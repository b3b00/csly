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
using bench.json;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Toolchains.InProcess;


namespace bench
{

    [MemoryDiagnoser]
    
    [Config(typeof(Config))]
    public class GenericLexerBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.Current.Value);
            }
        }

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
        
        public void TestJson()
        {
            var ignored = BenchedLexer.Tokenize(content);
        }



    }

}
