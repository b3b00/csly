using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NFluent;
using ParserTests.Issue311;
using sly.lexer;
using Xunit;

namespace ParserTests.Issue328;

public class Issue328Tests
{
    [Fact]
    public void TestIssue328()
    {
        var lexb = LexerBuilder.BuildLexer<Issue328Token>();
        Check.That(lexb).IsOk();
        var lexer = lexb.Result;
        var lex = lexer.Tokenize("XL(Key Metrics/C455) >> String: @working_adj.amount");
        Check.That(lex).IsOkLexing();
        var tokens = lex.Tokens;
        
        var z = tokens.MainTokens().Take(3).Extracting(x => (x.TokenID, x.StringWithoutQuotes)).ToList();
       
        var expectations = new List<(Issue328Token, string)>()
        {
            (Issue328Token.START_X, "XL"),
            (Issue328Token.OPS, "(Key Metrics/C455) "),
            (Issue328Token.INSTALL, ">>"),
            (Issue328Token.CONVERT, "String"),
            (Issue328Token.COLON, ":"),
            (Issue328Token.START_D, "@"),
            (Issue328Token.OPS, "working_adj.amount"),
        };
        
        
        Check.That(tokens.MainTokens().Take(7).Extracting(x => (x.TokenID, x.StringWithoutQuotes))).ContainsExactly(expectations);
       
    }

}