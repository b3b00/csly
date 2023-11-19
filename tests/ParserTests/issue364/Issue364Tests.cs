using expressionparser;
using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue364;

public class Issue364Tests
{
    [Fact]
    public void TestIssue364()
    {
        ParserBuilder<ExpressionToken, int> Parser = new ParserBuilder<ExpressionToken, int>("en");
        ExpressionParser oparser = new ExpressionParser();
        
        var r = Parser.BuildParser(oparser,ParserType.LL_RECURSIVE_DESCENT,"expression");
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