using System;
using System.IO;
using bench.json;
using bench.json.model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using sly.parser;
using sly.parser.generator;

namespace benchCurrent
{

    [MemoryDiagnoser]
    
    [Config(typeof(Config))]
    public class JsonParserBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
            }
        }

        private Parser<JsonTokenGeneric,JSon> BenchedParser;

        private string content = "";

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(("SETUP"));
            content = File.ReadAllText("test.json");
            Console.WriteLine("json read.");
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            
            var result = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Console.WriteLine("parser built.");
            if (result.IsError)
            {
                Console.WriteLine("ERROR");
                result.Errors.ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("parser ok");
                BenchedParser = result.Result;
            }
            
            Console.WriteLine($"parser {BenchedParser}");
        }

        [Params(true,false)]
        public bool Memoize { get; set; }
        
        [Params(true,false)]
        public bool Broaden { get; set; }
        
        [Benchmark]
        
        public void TestJson()
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
