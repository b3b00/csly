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
using sly.buildresult;

namespace BenchCslies;

public enum J
{
    Big,
    Long,
    Wide,
    Deep,
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
    Parser<CslyJsonTokenGeneric,JSon> _stackParser;
    Parser<CslyJsonTokenGeneric,JSon> _fluentParser;

    GeneratedEbnfJsonGenericParserMain _genParser; 
    string _json = "";
    private string _bigJson;
    private string _longJson;
    private string _wideJson;
    private string _deepJson;

    public string GetJson(J j)
    {
        return j switch
        {
            J.Big => _bigJson,
            J.Long => _longJson,
            J.Deep => _deepJson,
            J.Wide => _wideJson
        };
    }
    
    
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
        
        File.WriteAllText("c:/tmp/csly.grammar.txt",r.Result.Configuration.Dump());

        r = builder.BuildParser(new CslyEbnfJsonGenericParser(), ParserType.EBNF_LL_STACK, "root");
        if (r.IsError)
        {
            foreach (var error in r.Errors)
            {
                Console.Error.WriteLine(error);
            }
            Environment.Exit(1);
        }
        File.WriteAllText("c:/tmp/introspectivefluentcsly.grammar.txt",r.Result.Configuration.Dump());

        _stackParser = r.Result;
        

        var fluentParserBuild = FluentParserBuild();

        File.WriteAllText("c:/tmp/fluent.grammar.txt",fluentParserBuild.Result.Configuration.Dump());
        
        _fluentParser = fluentParserBuild.Result;

        _json = JsonBuilder.BuildJson(4, 4, 3);
        
        _bigJson = JsonBuilder.BuildJson(4, 4, 3).ToString()!;
        _longJson = JsonBuilder.BuildJson(256, 1, 1).ToString()!;
        _wideJson = JsonBuilder.BuildJson(1, 1, 256).ToString()!;
        _deepJson = JsonBuilder.BuildJson(1, 256, 1).ToString()!;
        
        
        var instanceGen = new GeneratedEbnfJsonGenericParser();
        _genParser = new GeneratedEbnfJsonGenericParserMain(instanceGen); 
    


    }

    private static BuildResult<Parser<CslyJsonTokenGeneric, JSon>> FluentParserBuild()
    {
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
            
            
            .Production("object: ACCG[d] ACCD[d]", (args) =>
            {
                return new JObject();
            })
            .Production("object: ACCG[d] members ACCD[d]", (args) =>
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

        return fluentParserBuild;
    }

    //[Params(J.Big, J.Deep, J.Long, J.Wide)]
    [Params(J.Big, J.Deep)]
    public J Type { get; set; }
    
    
    [Benchmark]

    public void Csly()
    {
        var json = GetJson(Type);
        _cslyParser.Parse(json);
    }
    
    [Benchmark]
    public void Stack()
    {
        var json = GetJson(Type);
        _stackParser.Parse(json);
    }
    
    [Benchmark]
    public void Fluent()
    {
        var json = GetJson(Type);
        _fluentParser.Parse(json);
    }

    [Benchmark]

    public void Generated()
    {
        var json = GetJson(Type);
        _genParser.Parse(json);
    }
}