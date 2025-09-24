using NFluent;
using ParserTests.Issue574;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue527;

public class Issue527Test
{

    private Parser<Issue527Lexer, string> BuildParser(ParserType parserType)
    {
        var builder = new ParserBuilder<Issue527Lexer, string>("en");
        var result = builder.BuildParser(new Issue527Parser(), parserType,"root");
        Check.That(result).IsOk();
        return result.Result;
    }
    
    [Theory]
    [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
    [InlineData(ParserType.EBNF_LL_STACK)]
    public void Issue527Test_OK_with_ParseWithoutContext(ParserType parserType)
    {
        var parser = BuildParser(parserType);
        var result = parser.Parse("a a b");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("any(a,a) and boo(b)");
    }
    
    [Theory]
    [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
    [InlineData(ParserType.EBNF_LL_STACK)]
    public void Issue527Test_KO_with_ParseWithContext(ParserType parserType)
    {
        var parser = BuildParser(parserType);
        var lexed = parser.Lexer.Tokenize("a a b");
        Check.That(lexed).IsOkLexing();
        var result = parser.ParseWithContext(lexed.Tokens.MainTokens());
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("any(a,a) and boo(b)");
    }
}