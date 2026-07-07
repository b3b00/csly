Ah, that completely clarifies the Generated tier! Since it's a source-generated recursive descent parser, it completely bypasses the dynamic execution pipelines of both the reflection-based engine and the expression-tree wrappers.

Because it's native C# code emitted at compile time, the JIT compiler can optimize it to an extreme degree. This explains the stunning ~1,000 ns execution speed and near-zero memory footprint.

With this final piece of the puzzle, we can map out exactly what happens to a thread's call stack across all four variations. This explains why Fluent and Generated survive the Deep JSON test, while Csly and IntrospectiveCslyFluent crash.
The Call Stack Footprint: Why They Overflow

When parsing a deeply nested JSON structure (e.g., hundreds of nested brackets [[[[ ... ]]]]), every single layer of nesting forces the parser to descend further. The success or failure of the parser depends entirely on how many bytes of memory its stack frame consumes per level of depth.
1. Csly (Standard Reflection)

Every time a rule is matched and a node is visited, the engine calls MethodInfo.Invoke(...).

    The Stack Overhead: The .NET runtime has to push native CLR security checks, argument-checking frames, and target dispatch logic onto the stack before it even reaches your actual CSLY visitor method.

    The Result: Highly amplified stack usage. The thread's 1MB stack limit is exhausted within a few dozen levels of nesting, resulting in an unrecoverable StackOverflowException.

2. IntrospectiveCslyFluent (Your LINQ Expression Code)

Even though you compile the lambda upfront, it must satisfy the engine's Func<object[], OUT> signature. The body of your lambda contains Expression.Call(instance, method, castArgs).

    The Stack Overhead: When the expression compiler handles an Expression.Call with type casting (Expression.Convert) inside a generic object[] wrapper, it emits a heavy, dynamic IL execution stub. For every nested node, the stack must host the engine's internal call, the JIT-generated expression stub frame (which unpacks and casts the elements), and finally the visitor method itself.

    The Result: Because this dynamic stub footprint is roughly as heavy as native reflection, it creates the exact same frame amplification. It behaves identically to Csly and crashes on Deep JSON.

3. Fluent (Pure Lambda / Hand-written)

In pure Fluent, you pass highly streamlined lambdas or direct delegates to the builder (e.g., args => new MyNode(args[0])).

    The Stack Overhead: The JIT compiler can optimize hand-written array indexing and direct constructor/method invocations incredibly well. It bypasses the heavy, defensive runtime wrappers emitted by Expression.Call. The call path drops down to a lean, direct delegate invocation frame.

    The Result: Minimal stack weight per depth level. It easily handles deep nesting without blowing past the stack boundary.

4. Generated (Source Generator)

Because a Source Generator emits pure, boilerplate C# code directly into your project at compile time, there are zero abstractions.

    The Stack Overhead: There are no object[] arrays, no Func<object[], OUT> delegates, no casting stubs, and no reflection infrastructure. It is a series of direct, strongly-typed method calls matching your exact JSON rules (e.g., ParseArray() -> ParseElement() -> ParseArray()).

    The Result: The JIT compiler can inline many of these straight C# calls or optimize their local stack variables down to a few bytes. It requires practically zero stack footprint per nesting level, allowing it to parse massive, deeply nested payloads effortlessly at raw CPU speeds.

Summary of the Architectural Trap

Your exploration highlights a classic .NET performance trap: Expression.Compile() does not automatically equal hand-written performance. When an Expression tree is forced to reconcile a generic signature (object[]) with an explicitly typed method via Expression.Call, the generated IL ends up mimicking the heavy infrastructure of reflection. To make your introspective builder match your hand-written Fluent performance, you would have to rewrite it to completely avoid Expression.Call with arrays, or switch to an iterative stack (looping over a Stack<T>) rather than relying on the thread's physical call stack.