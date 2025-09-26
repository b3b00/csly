using NFluent;
using RelaxedVisitorTyping;
using sly.parser.generator;
using Xunit;

namespace ParserTests.relaxed;

public class RelaxedVisitorTypingTests
{
    
    [Fact]
    public void BnfRelaxedVisitorTest()
    {
        var parserInstance = new RelaxedExpressionParser();

        var builder = new ParserBuilder<RelaxedExpressionToken, Clause>();
        var buildResult = builder.BuildRelaxedParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "compare");
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var parseResult = parser.Parse("abcd.def -eq 12");
        Check.That(parseResult).IsOkParsing();
        Check.That(parseResult.Result.ToString()).Equals("abcd.def == 12");
    }
}