using NFluent;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue527;

public class Issue527Test
{

    private Parser<Issue527Lexer, string> BuildParser()
    {
        var builder = new ParserBuilder<Issue527Lexer, string>("en");
        var result = builder.BuildParser(new Issue527Parser(), ParserType.EBNF_LL_STACK,"root");
        Check.That(result).IsOk();
        return result.Result;
    }
    
    [Fact]
    public void Issue527Test_OK_with_ParseWithoutContext()
    {
        var parser = BuildParser();
        var result = parser.Parse("a a b");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("any(a,a) and boo(b)");
    }
    
    [Fact]
    public void Issue527Test_KO_with_ParseWithContext()
    {
        var parser = BuildParser();
        var lexed = parser.Lexer.Tokenize("a a b");
        Check.That(lexed).IsOkLexing();
        var result = parser.ParseWithContext(lexed.Tokens.MainTokens());
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("any(a,a) and boo(b)");
    }
}