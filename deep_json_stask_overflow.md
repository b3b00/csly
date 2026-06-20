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
