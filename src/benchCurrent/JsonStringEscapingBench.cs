using System;
using System.IO;
using bench.json;
using bench.json.model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using simpleExpressionParser;
using sly.parser;
using sly.parser.generator;

namespace benchCurrent;


[MemoryDiagnoser]
    
[Config(typeof(JsonStringEscapingBench.Config))]
public class JsonStringEscapingBench
{
    private class Config : ManualConfig
    {
        public Config()
        {
            var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
        }
    }
    
    private Parser<JsonTokenGenericNotEscaped, JSon> notEscapedJsonParser;
    private Parser<JsonTokenGenericEscaped, JSon> escapedJsonParser;
    
    private string content = "";
    
    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine(("SETUP"));
        content = File.ReadAllText("test.json");
        Console.WriteLine("json read.");
        var jsonParser = new EbnfJsonGenericParser();
        var builder = new ParserBuilder<JsonTokenGenericNotEscaped, JSon>();
            
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
            notEscapedJsonParser = result.Result;
        }
            
        var notJsonParser = new EbnfJsonGenericParserStringNotEscaped();
        var builderNot = new ParserBuilder<JsonTokenGenericEscaped, JSon>();
            
        var resultNot = builderNot.BuildParser(notJsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        Console.WriteLine("parser built.");
        if (resultNot.IsError)
        {
            Console.WriteLine("ERROR");
            resultNot.Errors.ForEach(Console.WriteLine);
        }
        else
        {
            Console.WriteLine("parser ok");
            escapedJsonParser = resultNot.Result;
        }
    }
    
    [Benchmark]
        
    public void TestNotEscapedJson()
    {
            
            
        if (notEscapedJsonParser == null)
        {
            Console.WriteLine("parser is null");
        }
        else
        {
            var ignored = notEscapedJsonParser.Parse(content);    
        }
    }
    
    [Benchmark]
        
    public void TestEscapedJson()
    {
            
            
        if (escapedJsonParser == null)
        {
            Console.WriteLine("parser is null");
        }
        else
        {
            var ignored = escapedJsonParser.Parse(content);    
        }
    }
    
}