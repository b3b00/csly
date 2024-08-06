using System.Collections.Generic;
using System.Linq;
using NFluent;
using sly.lexer;
using Xunit;

namespace ParserTests;


public enum AotLexer
{
    DOUBLE,
    IDENTIFIER,
    PLUS,
    INCREMENT,
    MINUS,
    TIMES,
    DIVIDE,
    LPAREN,
    RPAREN,
    FACTORIAL,
}

public class AotTests
{
    [Fact]
    public void AotLexerTest()
    {
        var builder = AotLexerBuilder<AotLexer>.NewBuilder<AotLexer>();
        var lexer = builder.Double(AotLexer.DOUBLE)
            .Sugar(AotLexer.PLUS, "+")
            .Keyword(AotLexer.PLUS,"PLUS")
            .Labeled("en","sum")
            .Labeled("fr","somme")
            .Sugar(AotLexer.MINUS, "-")
            .Sugar(AotLexer.TIMES, "*")
            .Sugar(AotLexer.DIVIDE, "/")
            .Sugar(AotLexer.LPAREN, "(")
            .Sugar(AotLexer.RPAREN, ")")
            .Sugar(AotLexer.FACTORIAL, "!")
            .Sugar(AotLexer.INCREMENT, "++")
            .AlphaNumId(AotLexer.IDENTIFIER)
            .Build();
        Check.That(lexer).IsNotNull();
        var lexed = lexer.Tokenize("1 + 1");
        Check.That(lexed).IsOkLexing();
        Check.That(lexed.Tokens.MainTokens()).CountIs(4);
        Check.That(lexed.Tokens.MainTokens().Take(3).Extracting(x => x.TokenID)).IsEqualTo(new[]
        {
            AotLexer.DOUBLE,
            AotLexer.PLUS,
            AotLexer.DOUBLE
        });
    }
}