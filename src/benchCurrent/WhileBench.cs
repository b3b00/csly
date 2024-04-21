using System;
using System.IO;
using bench.json;
using bench.json.model;
using benchCurrent.backtrack;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using sly.parser;
using sly.parser.generator;


namespace benchCurrent
{

    [MemoryDiagnoser]
    
    [Config(typeof(Config))]
    public class WhileBench
    {


        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
            }
        }

        private Parser<IndentedWhileTokenGeneric,WhileAST> BenchedParser;

        private string content = "";

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(("SETUP"));
            content = @"r:=1
i:=1
while i < 11 do 
    r := r * i
    print r
    print i
    i := i + 1
b := false 
if i == 589 then
    b := true
if i == 1857 then
    b := false
print b

r:=1
i:=1
while i < 11 do 
    r := r * i
    print r
    print i
    i := i + 1
b := false 
if i == 589 then
    b := true
if i == 1857 then
    b := false
print b

r:=1
i:=1
while i < 11 do 
    r := r * i
    print r
    print i
    i := i + 1
b := false 
if i == 589 then
    b := true
if i == 1857 then
    b := false
print b

r:=1
i:=1
while i < 11 do 
    r := r * i
    print r
    print i
    i := i + 1
b := false 
if i == 589 then
    b := true
if i == 1857 then
    b := false
print b
";
            
            var backTrackParser = new IndentedWhileParserGeneric();
            var builder = new ParserBuilder<IndentedWhileTokenGeneric, WhileAST>();
            
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
        
        public void TestWhile()
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
