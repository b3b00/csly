
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
using BenchmarkDotNet.Diagnosers;


namespace bench
{

    [MemoryDiagnoser]
    [HtmlExporter]
    [MarkdownExporter]
    [JsonExporter("-custom", indentJson: true, excludeMeasurements: true)]
    [Config(typeof(Config))]
    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    public class JsonParserBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
                Add(baseJob.WithNuGet("sly", "2.9.7.0").WithId("2.9.7.0"));
                // Add(baseJob.WithNuGet("sly", "2.9.9").WithId("2.9.9"));
                Add(baseJob.WithNuGet("sly", "3.0.1").WithId("3.0.1"));
                Add(EnvironmentAnalyser.Default);
            }
        }

        private static Parser<JsonTokenGeneric,JSon> BenchedParser;

        private static string content = "";

        [GlobalSetup]
        public void Setup()
        {
            content = File.ReadAllText("test.json");
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            
            var result = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            if (result.IsError)
            {
                result.Errors.ForEach(e => Console.WriteLine(e));
            }
            else
            {
                BenchedParser = result.Result;
            }
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
