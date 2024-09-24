using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
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
public class BenchExpressionCslyVsHand
{
    private Parser<GenericExpressionToken, double> _cslyParser;

    private string source = "1+2+3+4+5+6+7+8+9+10+11+12+13 *48! / + (true ? 42 : -42)";
    private BuildResult<ILexer<GenericExpressionToken>> _lexer;
    private int _loops = 1000;

    [GlobalSetup]
    public void Setup()
    {
        _lexer = LexerBuilder.BuildLexer<GenericExpressionToken>();
        ParserBuilder<GenericExpressionToken, double> builder = new ParserBuilder<GenericExpressionToken, double>();
        var build = builder.BuildParser(new GenericSimpleExpressionParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        _cslyParser = build.Result;
    }
    
    [Benchmark]
    public void Csly()
    {
        for (int i = 0; i < _loops; i++)
        {
            _cslyParser.Parse(source);
        }
    }
    
    [Benchmark]
    public void Hand()
    {
        for (int i = 0; i < _loops; i++)
        {
            var t = _lexer.Result.Tokenize("-1 + 1! + 4");
            var tokens = t.Tokens.MainTokens();
            var instance = new GenericSimpleExpressionParser();
            ExpressionParser parser = new ExpressionParser(instance);
            var r = parser.Root(tokens, 0);
            EBNFSyntaxTreeVisitor<GenericExpressionToken, double> visitor =
                new EBNFSyntaxTreeVisitor<GenericExpressionToken, double>(null, instance);
            var result = visitor.VisitSyntaxTree(r.Node);
        }
    }
}