using System;
using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.dateIssue;

public class Test
{
    [Fact]
    public void TestDateIssue()
    {
        var builder = new ParserBuilder<Issue554lexer, object>();
        var instance = new Issue554Parser();

        var buildParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, null, null);
        Check.That(buildParser).IsOk();
        var result = buildParser.Result.Parse("3.14 42 1977.03.30");
        Check.That(result).IsOkParsing();
    }
}