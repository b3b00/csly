using System.IO;
using bench.json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using sly.buildresult;
using sly.lexer;

namespace benchCurrent
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
            }
        }

        private ILexer<JsonTokenGenericEscaped> BenchedLexer;

        private string content = "";

        [GlobalSetup]
        public void Setup()
        {
            content = File.ReadAllText("test.json");

            var lexerRes = LexerBuilder.BuildLexer(new BuildResult<ILexer<JsonTokenGenericEscaped>>());
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
