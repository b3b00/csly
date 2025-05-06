using System;
using BenchmarkDotNet.Attributes;
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
    public class StackExpressionBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp80);
            }
        }

        private string expression = "";
        
        [GlobalSetup]
        public void Setup()
        {
            expression = GetExpression(1000);
        }
        
        [Params(ParserType.LL_RECURSIVE_DESCENT,ParserType.LL_STACK )]
        public ParserType parserType { get; set; }

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


        [Benchmark]
        public void BenchLargeExpression()
        {
            var instance = new ExpressionParser();
            ParserBuilder<expressionparser.ExpressionToken, int> builder =
                new ParserBuilder<expressionparser.ExpressionToken, int>();
            var parser = builder.BuildParser(instance, parserType, "expression");
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
