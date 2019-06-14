using System.IO;
using benchCurrent.json;
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
