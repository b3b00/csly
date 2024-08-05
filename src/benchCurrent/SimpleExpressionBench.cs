using System;
using System.IO;
using bench.json;
using bench.json.model;
using benchCurrent.backtrack;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using expressionparser;
using simpleExpressionParser;
using sly.parser;
using sly.parser.generator;

namespace benchCurrent
{

    [MemoryDiagnoser]
    
    [Config(typeof(Config))]
    public class SimpleExpressionBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
            }
        }

        private Parser<ExpressionToken, double> BenchedParser;

        private string content = "";

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(("SETUP"));
            content = "1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17+18+19+20";
            
            SimpleExpressionParser p = new SimpleExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, double>();
            
            var result = builder.BuildParser(p, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            
            if (result.IsError)
            {
                result.Errors.ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("parser ok");
                BenchedParser = result.Result;
            }
        }

        [Params(true,false)]
        public bool Memoize { get; set; }
        
        [Params(true,false)]
        public bool Broaden { get; set; }
        
        [Benchmark]
        
        public void TestBackTrack()
        {
            if (BenchedParser == null)
            {
                Console.WriteLine("parser is null");
            }
            else
            {
                BenchedParser.Configuration.UseMemoization = Memoize;
                BenchedParser.Configuration.BroadenTokenWindow = Broaden;
                var ignored = BenchedParser.Parse(content);
            }
        }



    }

}
