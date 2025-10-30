using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.llparser.bnf;

namespace RuleCompilationBenchmark
{
    /// <summary>
    /// Benchmark comparing parsing performance with and without rule compilation
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class RuleCompilationBenchmarks
    {
        private Parser<ExpressionToken, ExpressionNode> _parserWithoutCompilation;
        private Parser<ExpressionToken, ExpressionNode> _parserWithCompilation;
        
        private List<string> _simpleExpressions;
        private List<string> _mediumExpressions;
        private List<string> _complexExpressions;

        [GlobalSetup]
        public void Setup()
        {
            // Build parser without compilation
            var parserBuilder = new ParserBuilder<ExpressionToken, ExpressionNode>();
            var buildResult = parserBuilder.BuildParser(
                new SimpleExpressionParser(),
                ParserType.EBNF_LL_RECURSIVE_DESCENT,
                "expression"
            );

            if (buildResult.IsError)
            {
                throw new Exception($"Failed to build parser: {string.Join(", ", buildResult.Errors)}");
            }

            _parserWithoutCompilation = buildResult.Result;

            // Build parser with compilation
            var buildResult2 = parserBuilder.BuildParser(
                new SimpleExpressionParser(),
                ParserType.EBNF_LL_RECURSIVE_DESCENT,
                "expression"
            );

            if (buildResult2.IsError)
            {
                throw new Exception($"Failed to build parser: {string.Join(", ", buildResult2.Errors)}");
            }

            _parserWithCompilation = buildResult2.Result;
            
            // Enable rule compilation
            if (_parserWithCompilation.SyntaxParser is RecursiveDescentSyntaxParser<ExpressionToken, ExpressionNode> rdParser)
            {
                rdParser.EnableRuleCompilation();
            }

            // Prepare test data
            _simpleExpressions = new List<string>();
            _mediumExpressions = new List<string>();
            _complexExpressions = new List<string>();

            // Generate simple expressions: "1 + 2", "3 * 4", etc.
            for (int i = 0; i < 10; i++)
            {
                _simpleExpressions.Add($"{i} + {i + 1}");
            }

            // Generate medium expressions: "1 + 2 * 3 - 4 / 2"
            for (int i = 0; i < 10; i++)
            {
                _mediumExpressions.Add($"{i} + {i + 1} * {i + 2} - {i + 3} / 2");
            }

            // Generate complex expressions with parentheses: "((1 + 2) * (3 + 4)) / ((5 - 6) + (7 * 8))"
            for (int i = 0; i < 10; i++)
            {
                _complexExpressions.Add(
                    $"(({i} + {i + 1}) * ({i + 2} + {i + 3})) / (({i + 4} - {i + 5}) + ({i + 6} * {i + 7}))"
                );
            }
        }

        // ========== Simple Expressions Benchmarks ==========

        [Benchmark(Description = "Simple expressions WITHOUT compilation")]
        public void SimpleExpressions_WithoutCompilation()
        {
            foreach (var expression in _simpleExpressions)
            {
                var result = _parserWithoutCompilation.Parse(expression);
                if (result.IsError)
                    throw new Exception("Parse error");
            }
        }

        [Benchmark(Description = "Simple expressions WITH compilation")]
        public void SimpleExpressions_WithCompilation()
        {
            foreach (var expression in _simpleExpressions)
            {
                var result = _parserWithCompilation.Parse(expression);
                if (result.IsError)
                    throw new Exception("Parse error");
            }
        }

        // ========== Medium Expressions Benchmarks ==========

        [Benchmark(Description = "Medium expressions WITHOUT compilation")]
        public void MediumExpressions_WithoutCompilation()
        {
            foreach (var expression in _mediumExpressions)
            {
                var result = _parserWithoutCompilation.Parse(expression);
                if (result.IsError)
                    throw new Exception("Parse error");
            }
        }

        [Benchmark(Description = "Medium expressions WITH compilation")]
        public void MediumExpressions_WithCompilation()
        {
            foreach (var expression in _mediumExpressions)
            {
                var result = _parserWithCompilation.Parse(expression);
                if (result.IsError)
                    throw new Exception("Parse error");
            }
        }

        // ========== Complex Expressions Benchmarks ==========

        [Benchmark(Description = "Complex expressions WITHOUT compilation")]
        public void ComplexExpressions_WithoutCompilation()
        {
            foreach (var expression in _complexExpressions)
            {
                var result = _parserWithoutCompilation.Parse(expression);
                if (result.IsError)
                    throw new Exception("Parse error");
            }
        }

        [Benchmark(Description = "Complex expressions WITH compilation")]
        public void ComplexExpressions_WithCompilation()
        {
            foreach (var expression in _complexExpressions)
            {
                var result = _parserWithCompilation.Parse(expression);
                if (result.IsError)
                    throw new Exception("Parse error");
            }
        }

        // ========== Single Parse Benchmarks (for detailed analysis) ==========

        [Benchmark(Description = "Single parse WITHOUT compilation")]
        public void SingleParse_WithoutCompilation()
        {
            var result = _parserWithoutCompilation.Parse("(1 + 2) * (3 + 4) / (5 - 6)");
            if (result.IsError)
                throw new Exception("Parse error");
        }

        [Benchmark(Description = "Single parse WITH compilation")]
        public void SingleParse_WithCompilation()
        {
            var result = _parserWithCompilation.Parse("(1 + 2) * (3 + 4) / (5 - 6)");
            if (result.IsError)
                throw new Exception("Parse error");
        }

        // ========== Repeated Parse Benchmarks (shows compilation amortization) ==========

        [Benchmark(Description = "1000 parses WITHOUT compilation")]
        public void RepeatedParse_WithoutCompilation()
        {
            var expression = "1 + 2 * 3";
            for (int i = 0; i < 1000; i++)
            {
                var result = _parserWithoutCompilation.Parse(expression);
                if (result.IsError)
                    throw new Exception("Parse error");
            }
        }

        [Benchmark(Description = "1000 parses WITH compilation")]
        public void RepeatedParse_WithCompilation()
        {
            var expression = "1 + 2 * 3";
            for (int i = 0; i < 1000; i++)
            {
                var result = _parserWithCompilation.Parse(expression);
                if (result.IsError)
                    throw new Exception("Parse error");
            }
        }
    }
}

