``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.19042
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.210
  [Host]     : .NET Core 2.1.30 (CoreCLR 4.6.30411.01, CoreFX 4.6.30411.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.30 (CoreCLR 4.6.30411.01, CoreFX 4.6.30411.02), 64bit RyuJIT


```
|     Method |     Mean |     Error |    StdDev |    Median |     Gen 0 |    Gen 1 | Gen 2 | Allocated |
|----------- |---------:|----------:|----------:|----------:|----------:|---------:|------:|----------:|
| TestInvoke | 10.67 ms | 0.2947 ms | 0.8457 ms | 10.586 ms | 1000.0000 | 484.3750 |     - |   5.82 MB |
| TestLambda | 10.18 ms | 0.5755 ms | 1.6603 ms |  9.482 ms | 1000.0000 | 484.3750 |     - |   5.74 MB |
