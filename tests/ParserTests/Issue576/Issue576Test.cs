using NFluent;
using ParserTests;
using sly.parser.generator;
using Xunit;

namespace issue576;

public class Issue576Test
{
    [Fact]
    public void TestIssue576()
    {
        ParserBuilder<Issue576Lexer, object> builder = new ParserBuilder<Issue576Lexer, object>("en");
        var buildResult = builder.BuildParser(new Issue576Parser(),ParserType.EBNF_LL_RECURSIVE_DESCENT,"NTSection");
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        var parsed = parser.Parse("frozen int j = (i as number) + 1;");
        Check.That(parsed).IsOkParsing();
    }
}