
using System.Diagnostics.CodeAnalysis;
using BenchCslies.parsers.json.csly;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchCslies.parsers.json.JsonModel;
using sly.lexer;
using sly.lexer.fluent;
using sly.parser;
using sly.parser.generator;
using generatedJsonParser;
using generatedJsonParser.generatedebnfjsongenericparser;

namespace BenchCslies;

public class JsonBuilder
{
    private static readonly Random _random = new();

    public static string BuildJson(int length, int depth, int width)
    {

        var items = Enumerable.Repeat(1, length)
            .Select<int, string>(_ => BuildObject(depth, width));
        return "["+string.Join("," , items)+"]";
    }

    private static string BuildObject(int depth, int width)
    {
        if (depth == 0)
        {
            return RandomString(6);
        }
        var properties = Enumerable.Repeat(1, width)
            .Select(_ => $"{RandomString(5)} : {BuildObject(depth - 1, width)}");

        return "{"+string.Join("," + Environment.NewLine, properties)+Environment.NewLine+"}";
    }

    [SuppressMessage("security", "CA5394:Use cryptographically secure random number generators", Justification = "Test code")]
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return "\""+new string(
            Enumerable
                .Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)])
                .ToArray()
        )+"\"";
    }
}

[MemoryDiagnoser]
    
[Config(typeof(CsliesBench.Config))]
public class CsliesBench
{
    private class Config : ManualConfig
    {
        public Config()
        {
            var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp70);
        }
    }

    Parser<CslyJsonTokenGeneric,JSon> _cslyParser;
    Parser<CslyJsonTokenGeneric,JSon> _fluentParser;

    GeneratedEbnfJsonGenericParserMain _genParser; 
    string _json = "";

    [GlobalSetup]
    public void Setup()
    {
        ParserBuilder<CslyJsonTokenGeneric,JSon> builder = new ParserBuilder<CslyJsonTokenGeneric,JSon>();
        var instance = new CslyEbnfJsonGenericParser();
        var r = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT,"root");
        if (r.IsError)
        {
            foreach (var error in r.Errors)
            {
                Console.Error.WriteLine(error);
            }
            Environment.Exit(1);
        }
        _cslyParser = r.Result;

        

        var fluentLexerBuilder = FluentLexerBuilder<CslyJsonTokenGeneric>.NewBuilder()
            .Int(CslyJsonTokenGeneric.INT)
            .Double(CslyJsonTokenGeneric.DOUBLE)
            .String(CslyJsonTokenGeneric.STRING)

            .Keyword(CslyJsonTokenGeneric.NULL, "null")
            .Keyword(CslyJsonTokenGeneric.BOOLEAN, ["true", "false"])

            .Sugar(CslyJsonTokenGeneric.ACCG, "{")
            .Sugar(CslyJsonTokenGeneric.ACCD, "}")
            .Sugar(CslyJsonTokenGeneric.CROG, "[")
            .Sugar(CslyJsonTokenGeneric.CROD, "]")
            .Sugar(CslyJsonTokenGeneric.COMMA, ",")
            .Sugar(CslyJsonTokenGeneric.COLON, ":");
        
        var fluentParserBuild = FluentEBNFParserBuilder<CslyJsonTokenGeneric,JSon>.NewBuilder("root")
            .Production("root : value", (args) =>
            {
                return args[0] as JSon;
            })
            .Production("value : STRING", (args) =>
            {
                return new JValue((args[0] as Token<CslyJsonTokenGeneric>).StringWithoutQuotes);
            })
            .Production("value : DOUBLE", (args) =>
            {
                return new JValue((args[0] as Token<CslyJsonTokenGeneric>).DoubleValue);
            })
            .Production("value : INT", (args) =>
            {
                return new JValue((args[0] as Token<CslyJsonTokenGeneric>).IntValue);
            })
            .Production("value : BOOLEAN", (args) =>
            {
                return new JValue(bool.Parse((args[0] as Token<CslyJsonTokenGeneric>).Value));
            })
            .Production("value : NULL", (args) =>
            {
                return new JNull();
            })
            .Production("value : object", (args) =>
            {
                return args[0] as JSon;
            })
            .Production("root : list", (args) =>
            {
                return args[0] as JSon;
            })
            
            
            .Production("object: CROG[d] CROD[d]", (args) =>
            {
                return new JObject();
            })
            .Production("object: CROG[d] members CROD[d]", (args) =>
            {
                return args[0] as JSon;
            })
            
            
            .Production("list : CROG[d] CROD[d]", (args) =>
            {
                return new JList();
            })
            .Production("list : CROG[d] listElements CROD[d]", (args) =>
            {
                return args[0] as JSon;
            })
            
            .Production("listElements: value additionalValue*", (args) =>
            {
                var values = new JList(args[0] as JSon);
                values.AddRange(args[1] as List<JSon>);
                return values;
            })
            .Production("additionalValue: COMMA[d] value", (args) =>
            {
                return args[0] as JSon;
            })
            
            .Production("members: property additionalProperty*", (args) =>
            {
                var value = new JObject();
                value.Merge(args[0] as JObject);
                foreach (var p in (args[1] as List<JSon>)) value.Merge((JObject) p);
                return value;
            })
            .Production("additionalProperty: COMMA[d] property", (args) =>
            {
                return args[0] as JSon;
            })
            .Production("property: STRING COLON[d] value", (args) =>
            {
                var key = (Token<CslyJsonTokenGeneric>) args[0];
                var value = (JSon) args[1];
                return new JObject(key.StringWithoutQuotes, value);
            })
            .WithLexerbuilder(fluentLexerBuilder)
            .BuildParser();
        if (fluentParserBuild.IsError)
        {
            foreach (var error in fluentParserBuild.Errors)
            {
                Console.Error.WriteLine(error.Message);
            }
            Environment.Exit(1);
        }

        _fluentParser = fluentParserBuild.Result;

        _json = JsonBuilder.BuildJson(4, 4, 3);
        File.WriteAllText("./bench.json", _json);
        
        var instanceGen = new GeneratedEbnfJsonGenericParser();
        _genParser = new GeneratedEbnfJsonGenericParserMain(instanceGen); 
    


    }

    [Benchmark]

    public void TestCsly()
    {
        _cslyParser.Parse(_json);
    }
    
    [Benchmark]

    public void TestFluent()
    {
        _fluentParser.Parse(_json);
    }

    [Benchmark]

    public void TestGenerated()
    {
        _genParser.Parse(_json);
    }
}