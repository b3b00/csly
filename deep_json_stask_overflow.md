# Analysis of StackOverflowException with Deep JSON Parsing in `csly`

This document details why the `Csly` parser fails with a `StackOverflowException` when parsing deeply nested JSON, why `Fluent` initially appeared to succeed, and how to successfully parse deep structures within the default 1MB thread stack limit.

## 1. Why the Dynamic Parsers (`Csly` and `Fluent`) Stack Overflow

The `J.Deep` scenario creates a JSON object that is nested 256 levels deep (e.g., `{"key": {"key": ... }}`). 

The dynamic parsers in `sly` (`CslyEbnfJsonGenericParser` and the Fluent parser) evaluate the EBNF grammar as an abstract syntax tree at runtime. For every nested element in the JSON, the parser relies on generalized recursive methods (like `ParseNonTerminal()` and `Parse()`). 

These generic interpreter methods have a large "stack footprint". They allocate state variables, tracking lists for errors, ambiguities, and alternative nodes directly in the call stack for every recursive step. When parsing `J.Deep`, we observed that 256 levels of JSON nesting blows the default Windows thread stack size (1MB). This implies that each level of dynamic recursion consumes around ~4KB of stack memory.

## 2. Why `Fluent` Initially "Succeeded"

The `Fluent(J.Deep)` method only seemed to succeed because it wasn't actually parsing the JSON. In the `FluentParserBuild()` method, the `object` production was incorrectly defined using array brackets (`CROG` / `CROD`) instead of curly braces (`ACCG` / `ACCD`):

```csharp
// Incorrect original code in BenchCslies.cs
.Production("object: CROG[d] CROD[d]", ...)
.Production("object: CROG[d] members CROD[d]", ...)
```

Because of this bug, when it tried to parse `{"key": ... }`, it immediately failed at the first `{` character. Since it errored out right away, it never recursed, its stack depth stayed at 1, and it avoided the `StackOverflowException`. Once the brackets were fixed to curly braces, `Fluent` stack overflowed exactly like `Csly`.

## 3. How to Parse Successfully within the 1MB Stack Limit

To successfully parse a depth of 256 within the default 1MB stack limit, the most practical and performant solution is to **use the Source Generator** (`GeneratedEbnfJsonGenericParserMain`). 

Benchmark tests show that the `Generated` parser effortlessly succeeds for `J.Deep` and runs significantly faster.

### Why the Generated Parser Works:
By using the C# Source Generator (`csly.generator`), the grammar is compiled ahead of time into explicit, highly-optimized C# methods. Because it doesn't need to use the generalized interpreter loop, the stack footprint for each recursive call shrinks dramatically (from ~4KB down to just a few dozen bytes). 

This allows the generated parser to handle a depth of 256 using only a tiny fraction (roughly 10-20KB) of the 1MB stack limit, entirely avoiding the `StackOverflowException`.

### Alternative: Custom Thread
If you absolutely must use the dynamic parsers (`Csly`/`Fluent`) in production for deeply nested JSON, the only robust workaround in C# is to spawn a dedicated thread with a larger stack size (e.g., 10MB):

```csharp
var thread = new Thread(() => {
    _cslyParser.Parse(json); 
}, 10 * 1024 * 1024); // 10MB stack size

thread.Start();
thread.Join();
```
*Note: This approach will not easily work with BenchmarkDotNet, as it runs benchmarks on ThreadPool threads constrained to the default 1MB limit.*

## 4. Can We Reduce the Generic Parse Method Stack Footprint?

Using pre-allocated data structures on the heap (like an `ObjectPool<List<ISyntaxNode>>`) is excellent for reducing Garbage Collection (GC) pressure and speeding up the parser, but **it will not shrink the call stack footprint significantly**. The stack still has to hold the 8-byte reference/pointer to that heap object, regardless of whether it's newly allocated or pulled from a pool.

If you want to modify `sly`'s dynamic parsers to survive deeper recursion within the 1MB limit, the stack footprint must be reduced architecturally:

### A. Consolidate Method Arguments (Minor Stack Savings)
Currently, methods like `Parse` pass around ~40 bytes of state per call:
```csharp
public override SyntaxParseResult<IN, OUT> Parse(
    Token<IN>[] tokens, 
    Rule<IN, OUT> rule, 
    int position,
    string nonTerminalName, 
    SyntaxParsingContext<IN, OUT> parsingContext)
```
By wrapping these into a single lightweight struct or class (e.g., `ParseState`), you can reduce the argument footprint to just 8 bytes per recursive call.

### B. Break Up Giant Methods (Major Stack Savings)
The biggest contributor to the ~4KB per-level stack consumption is likely the giant `switch` statement inside `EBNFRecursiveDescentSyntaxParser.Parse()`. 

Even if code only executes one branch of a `switch`, the .NET JIT compiler often reserves stack slots for **all** local variables declared across all the branches. This causes the method to have a massive stack frame (hundreds of bytes per call).
* **The Fix:** Extract the logic inside each `switch` case into its own separate method (e.g., `ParseChoiceClause()`, `ParseOptionClause()`). This prevents the JIT from allocating stack space for variables you aren't using in the current execution path.

### C. The Ultimate Fix: Move the Call Stack to the Heap (Iterative Parsing)
If you truly want to parse infinitely deep JSON (or any grammar) without blowing the 1MB stack, you have to stop using C#'s native method recursion entirely. This is exactly what is meant by "using preallocated data structures in the heap".

You would rewrite the parser loop to use a `Stack<T>` object allocated on the heap (often called "Trampolining" or a "State Machine" parser). Instead of `Parse()` calling `ParseNonTerminal()` which calls `Parse()`, you push state to the heap:

```csharp
var executionStack = new Stack<ParseTask>(); // Allocated on the HEAP
executionStack.Push(new ParseTask(rootRule, 0));

while (executionStack.Count > 0) 
{
    var task = executionStack.Pop();
    // Evaluate the clause...
    // If it requires parsing a sub-rule, push it to the stack instead of calling a method:
    executionStack.Push(new ParseTask(childRule, task.Position));
}
```
Because the `Stack<T>` grows on the Heap (which has gigabytes of space) rather than the thread's Call Stack (which is strictly 1MB), you completely eliminate the possibility of a `StackOverflowException`, no matter how deep the parsing goes.
