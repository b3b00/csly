using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using expressionparser;
using simpleExpressionParser;
using sly.parser;
using sly.parser.generator;
using ExpressionToken = simpleExpressionParser.ExpressionToken;

namespace benchCurrent
{

    [MemoryDiagnoser]
    
    [Config(typeof(Config))]
    //[Config(typeof(ConfigWithPercentage))]
    public class StackEbnfExpressionBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp80);
            }
        }

        private string expression = "";
        
        [GlobalSetup]
        public void Setup()
        {
            expression = GetExpression(1000);
        }
        
        //[Params(ParserType.LL_RECURSIVE_DESCENT,ParserType.LL_STACK )]
        //public ParserType parserType { get; set; }

        public string GetExpression(int max)
        {
            var rnd = new Random();
            //int width = rnd.Next(100, max);
            char[] ops = new[] { '+', '-', '*' };
            var getOp = () => ops[rnd.Next(0, ops.Length)];
            var expr = rnd.Next(0, 100).ToString();
            for (int i = 0; i < max; i++)
            {
                var op = getOp();
                var right = rnd.Next(0, 100);
                expr += $"{op} {right}";

            }
Console.WriteLine(expr);
            return expr;
        }


        [Benchmark(Baseline = true)]
        public void recursive() => BenchLargeExpression(ParserType.EBNF_LL_RECURSIVE_DESCENT);

        [Benchmark]
        public void stacked() => BenchLargeExpression(ParserType.EBNF_LL_STACK);
        
        public void BenchLargeExpression(ParserType type)
        {
            var instance = new SimpleExpressionParser();
            ParserBuilder<expressionparser.ExpressionToken, double> builder =
                new ParserBuilder<expressionparser.ExpressionToken, double>();
            var parser = builder.BuildParser(instance, type, "root");
            if (!parser.IsOk)
            {
                foreach (var error in parser.Errors)
                {
                    Console.WriteLine(error.Message);
                }
                Environment.Exit(1);
            }
            var r = parser.Result.Parse(expression);
            ;
        }
        
        



    }

}
