
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

# JSON (generated)

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2
DefaultJob : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2


| Method | Mean     | Error    | StdDev    | Median   | Gen0       | Gen1      | Gen2      | Allocated |
|------- |---------:|---------:|----------:|---------:|-----------:|----------:|----------:|----------:|
| Csly   | 331.5 ms | 37.97 ms | 111.95 ms | 352.7 ms | 20000.0000 | 8000.0000 | 2000.0000 | 110.64 MB |
| Hand   | 149.4 ms |  6.88 ms |  19.84 ms | 140.7 ms | 18500.0000 | 8500.0000 | 3500.0000 | 104.51 MB |

// * Warnings *
MultimodalDistribution
BenchJsonCslyVsHand.Csly: Default -> It seems that the distribution is bimodal (mValue = 3.33)

Only 1/2 but that was expected as json is almost regular. Still the speed boost is here.
Memory alloc is on par, which is expected as json parser produces a large AST where expression only manipulates numbers 
and do not allocate much memory for the final result.

A better comparison would to isolate the lexing+syntax parsing and ignore the syntax tree visit.
furthermore as the json is quite large visiting the syntax tree may dwarf the parse time.


# JSON (generated) : lex and parse only (no tree visit)

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2
DefaultJob : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2


| Method | Mean     | Error    | StdDev   | Median   | Gen0       | Gen1      | Gen2      | Allocated |
|------- |---------:|---------:|---------:|---------:|-----------:|----------:|----------:|----------:|
| Csly   | 116.9 ms |  4.30 ms | 12.48 ms | 114.7 ms | 17000.0000 | 7400.0000 | 2200.0000 |  96.99 MB |
| Hand   | 147.1 ms | 23.50 ms | 69.30 ms | 104.9 ms | 15666.6667 | 9333.3333 | 3666.6667 |  92.12 MB |

Ooooh !!! maybe the generated syntax tree for hand is bad ? => comparison : No, all is right.

## limiting to 1 loop (instead of 1000, let benchmarkdotnet do its job) : limited job : lex + parse

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2
DefaultJob : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2



| Method | Mean     | Error    | StdDev   | Gen0      | Gen1     | Gen2     | Allocated |
|------- |---------:|---------:|---------:|----------:|---------:|---------:|----------:|
| Csly   | 10.98 ms | 0.210 ms | 0.196 ms | 1687.5000 | 750.0000 | 281.2500 |    9.7 MB |
| Hand   | 10.26 ms | 0.251 ms | 0.704 ms | 1578.1250 | 875.0000 | 343.7500 |   9.21 MB |


## limiting to 1 loop (instead of 1000, let benchmarkdotnet do its job) : full job : lex + parse + visit

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.400
[Host]     : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2
DefaultJob : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2


| Method | Mean     | Error    | StdDev   | Median   | Gen0      | Gen1      | Gen2     | Allocated |
|------- |---------:|---------:|---------:|---------:|----------:|----------:|---------:|----------:|
| Csly   | 22.27 ms | 0.433 ms | 0.532 ms | 22.12 ms | 2937.5000 | 1500.0000 | 468.7500 |  17.38 MB |
| Hand   | 15.81 ms | 0.637 ms | 1.847 ms | 15.23 ms | 1812.5000 |  828.1250 | 343.7500 |  10.45 MB |

