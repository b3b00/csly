using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Collections.Generic;


namespace VersionComparison
{
    /// <summary>
    /// Benchmark comparing local development version with NuGet 3.7.6
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class VersionComparisonBenchmarks
    {
        private NuGetVersion.ParserWrapper _nugetParser;
        private LocalVersion.ParserWrapper _localParser;
        
        private List<string> _simpleExpressions;
        private List<string> _mediumExpressions;
        private List<string> _complexExpressions;

        [GlobalSetup]
        public void Setup()
        {
            // Build NuGet parser (v3.7.6)
            _nugetParser = new NuGetVersion.ParserWrapper();

            // Build Local parser (current branch)
            _localParser = new LocalVersion.ParserWrapper();

            // Prepare test data
            _simpleExpressions = new List<string>();
            _mediumExpressions = new List<string>();
            _complexExpressions = new List<string>();

            // Generate simple expressions: "1 + 2", "3 * 4", etc.
            for (int i = 1; i <= 20; i++)
            {
                _simpleExpressions.Add($"{i} + {i + 1}");
            }

            // Generate medium expressions: "1 + 2 * 3 - 4 / 2"
            for (int i = 1; i <= 20; i++)
            {
                _mediumExpressions.Add($"{i} + {i + 1} * {i + 2} - {i + 3}");
            }

            // Generate complex expressions with parentheses
            for (int i = 1; i <= 20; i++)
            {
                _complexExpressions.Add(
                    $"(({i} + {i + 1}) * ({i + 2} + {i + 3})) - (({i + 4} - {i + 5}) + ({i + 6} * {i + 7}))"
                );
            }
        }

        // ========== Simple Expressions Benchmarks ==========

        [Benchmark(Description = "Simple expressions - NuGet 3.7.6")]
        public void SimpleExpressions_NuGet()
        {
            foreach (var expression in _simpleExpressions)
            {
                _nugetParser.Parse(expression);
            }
        }

        [Benchmark(Description = "Simple expressions - Current Branch")]
        public void SimpleExpressions_Local()
        {
            foreach (var expression in _simpleExpressions)
            {
                _localParser.Parse(expression);
            }
        }

        // ========== Medium Expressions Benchmarks ==========

        [Benchmark(Description = "Medium expressions - NuGet 3.7.6")]
        public void MediumExpressions_NuGet()
        {
            foreach (var expression in _mediumExpressions)
            {
                _nugetParser.Parse(expression);
            }
        }

        [Benchmark(Description = "Medium expressions - Current Branch")]
        public void MediumExpressions_Local()
        {
            foreach (var expression in _mediumExpressions)
            {
                _localParser.Parse(expression);
            }
        }

        // ========== Complex Expressions Benchmarks ==========

        [Benchmark(Description = "Complex expressions - NuGet 3.7.6")]
        public void ComplexExpressions_NuGet()
        {
            foreach (var expression in _complexExpressions)
            {
                _nugetParser.Parse(expression);
            }
        }

        [Benchmark(Description = "Complex expressions - Current Branch")]
        public void ComplexExpressions_Local()
        {
            foreach (var expression in _complexExpressions)
            {
                _localParser.Parse(expression);
            }
        }
    }
}

