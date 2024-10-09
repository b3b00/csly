using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using benchgen.jsonparser.JsonModel;
using benchgen.jsonparser;
using expressionparser;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using EbnfJsonGenericParser = benchgen.jsonparser.EbnfJsonGenericParser;
using JsonTokenGeneric = benchgen.jsonparser.JsonTokenGeneric;

namespace benchgen;

[MemoryDiagnoser]
[HtmlExporter]
[MarkdownExporter]
[JsonExporter("-custom", indentJson: true, excludeMeasurements: true)]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class BenchExpressionCslyVsHand
{
    private Parser<ExpressionToken, int> _cslyParser;

    private string _source = null;
    private BuildResult<ILexer<ExpressionToken>> _lexer;
    private int _loops = 1;

    [GlobalSetup]
    public void Setup()
    {
        _source = "1+2+3+4+5+6+7*8-9-10+11-12*13-14+15+16+17+18+19+20+21";
        _lexer = LexerBuilder.BuildLexer<ExpressionToken>();
        ParserBuilder<ExpressionToken, int> builder = new ParserBuilder<ExpressionToken, int>();
        var build = builder.BuildParser(new ExpressionParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "expression");
        _cslyParser = build.Result;
    }

    [Benchmark]
    public void Csly()
    {
        var x = _cslyParser.Parse(_source);
    }

    [Benchmark]
    public void Hand()
    {
        var t = _lexer.Result.Tokenize(_source);
        var tokens = t.Tokens.MainTokens();
        var instance = new ExpressionParser();
        ExpressionParserGenerator generator = new ExpressionParserGenerator(instance);
        var expression = generator.ParseExpression(_source);
    }
}