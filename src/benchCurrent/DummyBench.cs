using System;
using System.IO;
using bench.json;
using bench.json.model;
using benchCurrent.backtrack;
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
    public class BackTrackBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
            }
        }

        private Parser<BackTrackToken,string> BenchedParser;

        private string content = "";

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(("SETUP"));
            content = "funcA(funcC(B==2));";
            
            var backTrackParser = new BackTrackParser();
            var builder = new ParserBuilder<BackTrackToken, string>();
            
            var result = builder.BuildParser(backTrackParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "block");
            
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
