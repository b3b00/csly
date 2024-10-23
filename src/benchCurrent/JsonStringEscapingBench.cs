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
    
    private Parser<JsonTokenGeneric, JSon> escapedJsonParser;
    private Parser<JsonTokenGenericStringNotEscaped, JSon> unescapedJsonParser;
    
    private string content = "";
    
    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine(("SETUP"));
        content = File.ReadAllText("test.json");
        Console.WriteLine("json read.");
        var jsonParser = new EbnfJsonGenericParser();
        var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            
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
            escapedJsonParser = result.Result;
        }
            
        var notJsonParser = new EbnfJsonGenericParserStringNotEscaped();
        var builderNot = new ParserBuilder<JsonTokenGenericStringNotEscaped, JSon>();
            
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
            unescapedJsonParser = resultNot.Result;
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
    
    [Benchmark]
        
    public void TestUnescapedJson()
    {
            
            
        if (unescapedJsonParser == null)
        {
            Console.WriteLine("parser is null");
        }
        else
        {
            var ignored = unescapedJsonParser.Parse(content);    
        }
    }
    
}