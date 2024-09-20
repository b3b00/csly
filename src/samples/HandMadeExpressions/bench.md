
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

