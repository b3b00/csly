After "delegate" approach implemenation : 

| Method                  | Type | Mean           | Error        | StdDev        | Gen0     | Gen1     | Allocated  |
|------------------------ |----- |---------------:|-------------:|--------------:|---------:|---------:|-----------:|
| Csly                    | Big  | 3,421,604.9 ns | 68,145.85 ns | 134,513.27 ns | 671.8750 | 578.1250 | 8284.79 KB |
| IntrospectiveCslyFluent | Big  | 3,340,911.7 ns | 59,423.63 ns |  55,584.90 ns | 671.8750 | 562.5000 | 8321.57 KB |
| Fluent                  | Big  |   782,089.7 ns | 15,392.14 ns |  26,958.09 ns | 231.4453 | 159.1797 | 2843.84 KB |
| Generated               | Big  |     1,468.5 ns |     28.11 ns |      28.86 ns |   1.7223 |   0.1221 |   21.15 KB |
| Csly                    | Deep |             NA |           NA |            NA |       NA |       NA |         NA |
| IntrospectiveCslyFluent | Deep |             NA |           NA |            NA |       NA |       NA |         NA |
| Fluent                  | Deep |   342,682.9 ns |  6,597.78 ns |   8,102.66 ns | 121.0938 |  62.9883 | 1487.51 KB |
| Generated               | Deep |       819.8 ns |     16.39 ns |      35.63 ns |   0.8650 |   0.0296 |   10.59 KB |

The fact that the benchmark results remain practically unchanged even after switching to a pre-bound Delegate.CreateDelegate inside the LINQ expression tree is a fantastic, highly educational data point. It means we have successfully ruled out Expression.Call compilation overhead as the primary culprit, and it isolates the bottleneck down to a single, unyielding structural constraint.
The Concrete Truth: The Func<object[], OUT> Abstraction is the Ceiling

Since the performance, memory allocations, and Deep JSON stack overflow behavior of IntrospectiveCslyFluent are still a 100% match for standard Csly, we can conclusively deduce what is happening:

The .NET runtime treats an arbitrary unpack-and-cast loop inside a Func<object[], OUT> signature identically to standard reflection, regardless of how cleanly you emit the IL or delegates.

Here is the exact breakdown of why the delegate trick didn't change the numbers, and what makes your pure Fluent API fundamentally different.
## 1. The Real Reason the Delegate Optimization Changed Nothing

In the suggested optimization, we used Expression.Invoke(delegateConstant, castArgs). Under the hood, look at what the LINQ Expression compiler must do to execute that:

    It takes the incoming object[] args.

    For each parameter the target method needs, it emits code to index into the array (args[0], args[1]).

    It emits explicit cast instructions (Unbox_Any for value types or Castclass for reference types) to convert those objects back to the real types.

    It pushes those unpacked, casted variables onto the virtual execution stack.

    It finally invokes the target delegate.

Now, consider what the .NET Runtime's native MethodInfo.Invoke(instance, object[] parameters) does internally:

    It loops through the object[] parameters array.

    It checks types and unboxes/casts each element.

    It pushes them onto the physical thread stack.

    It calls the method.

Because the expression tree is structurally forced to perform the exact same sequence of array-indexing, type-casting, and stack-pushing operations on every single node visit, the compiled IL is functionally identical to what MethodInfo.Invoke does natively. The execution profile, garbage generation (from array management elsewhere), and stack frame sizes end up completely indistinguishable.

## 2. Why your pure Fluent variant is 4.5x faster

If Fluent and IntrospectiveCslyFluent share the exact same parsing engine, and the engine requires a Func<object[], OUT>, how is Fluent running in 782,089 ns while the introspective variant is stuck at 3,340,911 ns?

The secret lies in how the array is used (or avoided) in the pure Fluent implementation. When you write a pure Fluent parser manually, you write lambdas that typically handle the arguments in a highly linear, localized way. For example:
C#
```csharp
// Pure Fluent style:
builder.Production("value : INT", args => new IntNode((int)args[0]));
```

When you hardcode this, the compiler creates a highly specialized, isolated closure class. The JIT compiler optimizes manual casting and immediate allocation of a specific node spectacularly well. It doesn't have to emit the generic, robust, multi-argument metadata matching blocks required when generating code dynamically via reflection structures.

Furthermore, if your pure Fluent API has generic overloads behind the scenes that you are using without realizing it—such as Production<T1, T2>(..., Func<T1, T2, OUT> action)—then the pure Fluent variant isn't using object[] at all under the hood. It's passing strongly-typed arguments through .NET Generics. The moment generics are involved, allocations drop by 3x and speed increases by 400%, exactly as your benchmarks show.
Conclusion & The Ultimate Fix

This benchmark conclusively proves that any dynamic approach that attempts to automatically map an arbitrary reflection MethodInfo to a fixed Func<object[], OUT> pipeline will inherit the performance penalties of reflection. The overhead isn't the lookup or the invocation method itself; the overhead is the inevitable architectural tax of unpacking and casting an array of objects for every syntax node.

If you want IntrospectiveCslyFluent to achieve the blazing-fast speeds and deep-stack safety of your pure Fluent implementation, you cannot route the introspection through a generic object[] signature.

You have two choices to bridge the gap:

    Embrace the Source Generator (Generated): Your Generated parser proves that compile-time code generation is the absolute gold standard (~1,400 ns). Instead of trying to fix runtime introspection, lean heavily into the source generator to generate the fluent builder boilerplate at compile time.

    Switch to Type-Safe Combinators: Alter the engine's internal visit loop to pass a dedicated ParsingContext or a lightweight ReadOnlySpan of tokens/nodes rather than mapping values to arrays of objects.