using expressionparser;
using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue364;

public class Issue364Tests
{
    [Theory]
    [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
    [InlineData(ParserType.EBNF_LL_STACK)]
    public void TestIssue364(ParserType parserType)
    {
        ParserBuilder<ExpressionToken, int> Parser = new ParserBuilder<ExpressionToken, int>("en");
        ExpressionParser oparser = new ExpressionParser();
        
        var r = Parser.BuildParser(oparser,parserType,"expression");
        Check.That(r).IsOk();
        var parser = r.Result;
        var result = parser.Parse("1 + 1 ");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(2);
        result = parser.Parse("1 + 1 +");
        Check.That(result).Not.IsOkParsing();
        result = parser.Parse("1 + 1 + 1");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(3);

    }

}