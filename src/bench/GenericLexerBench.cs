using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using sly.lexer;
using sly.buildresult;
using System.IO;
using bench.json;
using BenchmarkDotNet.Analysers;


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
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
                Add(baseJob.WithNuGet("sly", "2.9.9").WithId("2.9.9"));
                Add(baseJob.WithNuGet("sly", "3.0.0").WithId("3.0.0"));
                Add(EnvironmentAnalyser.Default);
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
        
        public void TestJson() {
            var ignored = BenchedLexer.Tokenize(content).Tokens;
        }



    }

}
