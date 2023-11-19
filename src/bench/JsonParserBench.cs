
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



namespace bench
{

    [MemoryDiagnoser]
    
    [Config(typeof(Config))]
    public class JsonParserBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.Current.Value);
                Add(baseJob.WithNuGet("sly", "2.2.5.1").WithId("2.2.5.1"));
                Add(baseJob.WithNuGet("sly", "2.2.5.2").WithId("2.2.5.2"));
                Add(EnvironmentAnalyser.Default);
                Add(baseJob.WithNuGet("sly", "2.2.5.3").WithId("2.2.5.3"));
                Add(baseJob.WithNuGet("sly", "2.3.0.1").WithId("2.3.0.1"));
            }
        }

        private Parser<JsonTokenGeneric,JSon> BenchedParser;

        private string content = "";

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(("SETUP"));
            Console.ReadLine();
            content = File.ReadAllText("test.json");
            Console.WriteLine("json read.");
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            
            var result = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Console.WriteLine("parser built.");
            if (result.IsError)
            {
                Console.WriteLine("ERROR");
                result.Errors.ForEach(e => Console.WriteLine(e));
            }
            else
            {
                Console.WriteLine("parser ok");
                BenchedParser = result.Result;
            }
            
            Console.WriteLine($"parser {BenchedParser}");
        }

        [Benchmark]
        
        public void TestJson()
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
