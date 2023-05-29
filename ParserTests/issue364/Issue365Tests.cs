using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NFluent;
using ParserTests.Issue311;
using ParserTests.issue364;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue364;

public class Issue332Tests
{
    [Fact]
    public void TestIssue364()
    {
        ParserBuilder<Issue364Token, object> Parser = new ParserBuilder<Issue364Token, object>();
        Issue364Parser oparser = new Issue364Parser();
        var r = Parser.BuildParser(oparser,ParserType.EBNF_LL_RECURSIVE_DESCENT,"expression");
        Check.That(r).IsOk();
        var parser = r.Result;
        var result = parser.Parse("1 + 1 ");
        Check.That(result).IsOkParsing();
        result = parser.Parse("1 + 1 +");
        Check.That(result).Not.IsOkParsing();

    }

}