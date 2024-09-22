
# basic expressions + - * / ! 

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4780/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


 Method | Mean      | Error    | StdDev    | Median    | Gen0       | Gen1      | Allocated |
------- |----------:|---------:|----------:|----------:|-----------:|----------:|----------:|
 Csly   | 158.27 ms | 3.566 ms | 10.117 ms | 155.69 ms | 46000.0000 | 2000.0000 |  219.7 MB |
 Hand   |  13.48 ms | 1.481 ms |  4.366 ms |  11.24 ms |  5562.5000 |         - |  22.24 MB |


CPU : / 11 
Memory : / 9.8 => no Gen1

# basic expressions + ternary ?:

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4780/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


 Method | Mean      | Error    | StdDev    | Median    | Gen0       | Gen1      | Allocated |
------- |----------:|---------:|----------:|----------:|-----------:|----------:|----------:|
 Csly   | 170.99 ms | 8.318 ms | 22.772 ms | 161.28 ms | 46000.0000 | 2000.0000 | 222.27 MB |
 Hand   |  17.43 ms | 1.896 ms |  5.591 ms |  16.25 ms |  5562.5000 |         - |  22.24 MB |

CPU : / 10
Memory : / 10 => no Gen1 

# basic expressions + ternary : start using combinators

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4780/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


 Method | Mean      | Error    | StdDev    | Gen0       | Gen1      | Allocated |
------- |----------:|---------:|----------:|-----------:|----------:|----------:|
 Csly   | 192.62 ms | 9.027 ms | 26.188 ms | 46000.0000 | 3000.0000 | 222.27 MB |
 Hand   |  11.72 ms | 0.922 ms |  2.554 ms |  5562.5000 |         - |  22.24 MB |

CPU : / 17 !
Memory : / 10

# basic expressions + ternary : infix combinator

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4780/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


 Method | Mean      | Error    | StdDev   | Gen0       | Allocated |
------- |----------:|---------:|---------:|-----------:|----------:|
 Csly   | 116.84 ms | 2.358 ms | 6.652 ms | 53000.0000 |  212.5 MB |
 Hand   |  11.27 ms | 0.225 ms | 0.406 ms |  7093.7500 |  28.41 MB |


CPU : /10
Memory : /7.5


# extract basicparser for micro runtime

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


 Method | Mean      | Error    | StdDev    | Gen0       | Allocated |
------- |----------:|---------:|----------:|-----------:|----------:|
 Csly   | 128.70 ms | 6.358 ms | 18.243 ms | 53000.0000 |  212.5 MB |
 Hand   |  13.77 ms | 0.646 ms |  1.853 ms |  7109.3750 |  28.41 MB |


# discarded tokens

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


 Method | Mean      | Error    | StdDev    | Gen0       | Allocated |
------- |----------:|---------:|----------:|-----------:|----------:|
 Csly   | 129.75 ms | 8.391 ms | 24.074 ms | 53000.0000 |  212.5 MB |
 Hand   |  16.92 ms | 0.814 ms |  2.361 ms |  7093.7500 |  28.41 MB |
