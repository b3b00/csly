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
        private Parser<GenericExpressionToken, double> BenchedGenericParser;

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
            
            GenericSimpleExpressionParser gp = new GenericSimpleExpressionParser();
            var genericbuilder = new ParserBuilder<GenericExpressionToken, double>();
            
            var genericresult = builder.BuildParser(p, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            
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

     
        
        [Benchmark]
        
        public void TestExpressionRegex()
        {
            if (BenchedParser == null)
            {
                Console.WriteLine("parser is null");
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    var ignored = BenchedParser.Parse(content);
                }
            }
        }
        
        [Benchmark]
        
        public void TestExpressionGeneric()
        {
            if (BenchedGenericParser == null)
            {
                Console.WriteLine("parser is null");
            }
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    var ignored = BenchedGenericParser.Parse(content);
                }
            }
        }



    }

}
