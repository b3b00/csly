
using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

using System.IO;
using BenchmarkDotNet.Analysers;


using sly.parser;
using sly.parser.generator;
using bench.json;
using bench.json.model;
using bench.simpleExpressionParser;


namespace bench
{

    [MemoryDiagnoser]
    
    [Config(typeof(Config))]
    public class ExpressionParserBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp80);
                Add(baseJob.WithNuGet("sly", "3.3.5").WithId("3.3.5"));
                Add(baseJob.WithNuGet("sly", "3.4.0").WithId("3.4.0"));
                Add(EnvironmentAnalyser.Default);

            }
        }

        private Parser<GenericExpressionToken,double> BenchedParser;

        private string content = "1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17+18*19/20";

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(("SETUP"));
//            Console.ReadLine();
            content = File.ReadAllText("test.json");
            Console.WriteLine("json read.");
            var jsonParser = new GenericSimpleExpressionParser();
            var builder = new ParserBuilder<GenericExpressionToken, double>();
            
            var result = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Console.WriteLine("parser built.");
            if (result.IsError)
            {
                Console.WriteLine("ERROR");
                result.Errors.ForEach(e => Console.WriteLine(e.Message));
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("parser ok");
                BenchedParser = result.Result;
            }
            
            Console.WriteLine($"parser {BenchedParser}");
        }

        [Benchmark]
        
        public void TestExpression()
        {
            if (BenchedParser == null)
            {
                Console.WriteLine("parser is null");
            }
            else
            {
                var ignored = BenchedParser.Parse(content);    
            }
        }



    }

}
