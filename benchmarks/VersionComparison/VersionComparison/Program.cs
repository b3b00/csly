
using System;
using BenchmarkDotNet.Running;

namespace VersionComparison
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine("SLY Version Comparison Benchmark");
            Console.WriteLine("Comparing Current Branch vs NuGet 3.7.6");
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine();

            var summary = BenchmarkRunner.Run<VersionComparisonBenchmarks>();
            
            Console.WriteLine();
            Console.WriteLine("Benchmark completed!");
        }
    }
}

