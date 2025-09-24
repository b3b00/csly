using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NFluent;
using ParserTests.Issue311;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue332;

public class Issue332Tests
{
    [Theory]
    [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
    [InlineData(ParserType.EBNF_LL_STACK)]
    public void TestIssue328(ParserType parserType)
    {
        ParserBuilder<Issue332Token, object> Parser = new ParserBuilder<Issue332Token, object>();
        Issue332Parser oparser = new Issue332Parser();
        var r = Parser.BuildParser(oparser,parserType);
        Check.That(r).Not.IsOk();
        Check.That(r.Errors).Not.IsEmpty();
        var error = r.Errors.First();
        Check.That(error.Code).IsEqualTo(ErrorCodes.PARSER_LEFT_RECURSIVE);

    }

}