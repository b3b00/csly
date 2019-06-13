using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser;
using sly.parser.generator;
using sly.buildresult;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace bench
{

    [MemoryDiagnoser]
    
//     [Config(typeof(Config))]
    public class Bench
    {

 // Specify jobs with different versions of the same NuGet package to benchmark.
        // The NuGet versions referenced on these jobs must be greater or equal to the 
        // same NuGet version referenced in this benchmark project.
        // Example: This benchmark project references Newtonsoft.Json 9.0.1
//        private class Config : ManualConfig
//        {
//            public Config()
//            {
//                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.Current.Value);
//                Add(baseJob.WithNuGet("sly", "2.2.5.1").WithId("2.2.5.1"));
//                Add(baseJob.WithNuGet("sly", "2.2.5.2").WithId("2.2.5.2"));
//                Add(baseJob.WithNuGet("sly", "2.2.5.3").WithId("2.2.5.3"));
//                Add(baseJob.WithNuGet("sly", "2.3.0.1").WithId("2.3.0.1"));
//            }
//        }

        private ILexer<JsonTokenGeneric> BenchedLexer;

        private string content = "";

        [GlobalSetup]
        public void Setup()
        {
            content = File.ReadAllText("test.json");

            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<JsonTokenGeneric>>());
            if (lexerRes != null)
            {
                BenchedLexer = lexerRes.Result;
            }
        }

        [Benchmark]
        public void TestJson() {
            var ignored = BenchedLexer.Tokenize(content).ToList();
        }



    }

}
