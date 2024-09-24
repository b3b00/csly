using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using handExpressions.jsonparser;
using handExpressions.jsonparser.JsonModel;
using simpleExpressionParser;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;

namespace handExpressions;

[MemoryDiagnoser]
[HtmlExporter]
[MarkdownExporter]
[JsonExporter("-custom", indentJson: true, excludeMeasurements: true)]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class BenchJsonCslyVsHand
{
    private Parser<JsonTokenGeneric, JSon> _cslyParser;

    private string _source = null;
    private BuildResult<ILexer<JsonTokenGeneric>> _lexer;
    private int _loops = 1;

    [GlobalSetup]
    public void Setup()
    {
        _source = File.ReadAllText("C:/Users/olduh/dev/csly/src/bench2.4/test.json");
        _lexer = LexerBuilder.BuildLexer<JsonTokenGeneric>();
        ParserBuilder<JsonTokenGeneric, JSon> builder = new ParserBuilder<JsonTokenGeneric, JSon>();
        var build = builder.BuildParser(new EbnfJsonGenericParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        _cslyParser = build.Result;
    }
    
    [Benchmark]
    public void Csly()
    {
        for (int i = 0; i < _loops; i++)
        {
            var tokens = _cslyParser.Lexer.Tokenize(_source);
            //_cslyParser.SyntaxParser.Parse(tokens.Tokens.MainTokens());
            var x = _cslyParser.Parse(_source);
            //File.WriteAllText("c:/tmp/csly_json.txt",x?.Result?.ToJson());
        }
    }
    
    [Benchmark]
    public void Hand()
    {
        for (int i = 0; i < _loops; i++)
        {
            var t = _lexer.Result.Tokenize(_source);
            var tokens = t.Tokens.MainTokens();
            var instance = new EbnfJsonGenericParser();
            GeneratedEbnfJsonGenericParser parser = new GeneratedEbnfJsonGenericParser(instance);
            var r = parser.Root(tokens, 0);
            EBNFSyntaxTreeVisitor<JsonTokenGeneric, JSon> visitor =
                new EBNFSyntaxTreeVisitor<JsonTokenGeneric, JSon>(null, instance);
            var result = visitor.VisitSyntaxTree(r.Node);
            // File.WriteAllText("c:/tmp/hand_json.txt",result?.ToJson());
        }
    }
}