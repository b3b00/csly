using System.Collections.Generic;
using System.Linq;
using NFluent;
using sly.lexer;
using sly.lexer.fluent;
using Xunit;

namespace ParserTests;

public enum FluentToken
{
    NO,
    ID,
    STRING,
    INT,
    DOUBLE,
    CHAR,
    HELLO,
    WORLD,
    START_ISLAND,
    END_ISLAND,
    COMMA,
    ISLAND,
    DATE,
    HEXA,
    COMMENT,
    EXTENSION,
}

public class FluentTests
{
    [Fact]
    public void TestFluentLexerBuilder()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Int(FluentToken.INT)
            .Double(FluentToken.DOUBLE)
            .Hexa(FluentToken.HEXA,"0x")
            .Date(FluentToken.DATE,DateFormat.YYYYMMDD,'-')
            .String(FluentToken.STRING)
            .MultiLineComment(FluentToken.COMMENT,"/*","*/").OnChannel(Channels.Main)
            .Keyword(FluentToken.HELLO, "hello")
            .Keyword(FluentToken.WORLD, "world")
            .Sugar(FluentToken.START_ISLAND, ">>>").PushToMode("island")
            .UpTo(FluentToken.ISLAND, "<<<").WithModes("island")
            .Sugar(FluentToken.END_ISLAND, "<<<").WithModes("island").PopMode()
            .Sugar(FluentToken.COMMA, ",")
            
            .Build("en");
        Check.That(lexer).IsOk();
        
        var tokenized = lexer.Result.Tokenize(@"
2024-12-20
0xb3b00
hello, world
identifier
/*
Lorem ipsum dolor 
sit amet, consectetur.
*/
""string""
1
2.3
>>>
island content with int : 1, double :2.3, hexa 0xFFF, keyword : hello  
<<< 
");
        Check.That(tokenized).IsOkLexing();
        var all = tokenized.Tokens.AllExceptWhiteSpaces;
        Check.That(all).CountIs(14);
        Check.That(all.Extracting(x => x.TokenID).Take(13)).IsEqualTo(new List<FluentToken>()
        {
            FluentToken.DATE,
            FluentToken.HEXA,
            FluentToken.HELLO, FluentToken.COMMA, FluentToken.WORLD,
            FluentToken.ID,
            FluentToken.COMMENT,
            FluentToken.STRING,
            FluentToken.INT,
            FluentToken.DOUBLE,
            FluentToken.START_ISLAND, FluentToken.ISLAND, FluentToken.END_ISLAND
        });
    }
}