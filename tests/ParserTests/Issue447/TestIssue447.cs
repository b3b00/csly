using System.Collections.Generic;
using System.Linq;
using NFluent;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue447;


[Lexer(KeyWordIgnoreCase = true)]
public enum Issue447ExplicitTokenLexer
{
    EOS = 0,
    [Sugar(".")]
    DOT,
}

public class Issue447ExplicitTokenParser
{
    [Production("root : a*")]
    public string Root(List<string> items)
    {
        if (items != null && items.Any())
        {
            return string.Join(",",items);
        }

        return "";
    }

    [Production("a : 'a'")]
    public string A(Token<Issue447ExplicitTokenLexer> token)
    {
        return token.Value;
    }
}

public class TestIssue447
{
    [Fact]
    public void Issue447Test()
    {
        var lexerResult = LexerBuilder.BuildLexer<Issue447Lexer>();
        Check.That(lexerResult).IsOk();
        ;
        var r =lexerResult.Result.Tokenize("aa");
        Check.That(r).IsOkLexing();
        Check.That(r.Tokens).CountIs(3);
        Check.That(r.Tokens.Take(2).Extracting(x => x.TokenID))
            .IsEqualTo(new List<Issue447Lexer>() { Issue447Lexer.A, Issue447Lexer.A });
        Check.That(r.Tokens.Last().IsEOS).IsTrue();
    }

    [Fact]
    public void Issue447ExplicitTokenTest()
    {
        var issue443Parser = new Issue447ExplicitTokenParser();
        var builder = new ParserBuilder<Issue447ExplicitTokenLexer, string>();

        var parser = builder.BuildParser(issue443Parser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");

        Check.That(parser).IsOk();
        var r = parser.Result.Parse("aa");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo("a,a");
    }
}