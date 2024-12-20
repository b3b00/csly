using System.Collections.Generic;
using System.Linq;
using NFluent;
using sly.lexer;
using sly.lexer.fluent;
using sly.lexer.fsm;
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
    THREE_DOT,
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
    
    
    [Fact]
    public void TestFluentLexerBuilderWithExtension()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Date(FluentToken.DATE,DateFormat.YYYYMMDD,'-')
            .Extension(FluentToken.THREE_DOT)
            .UseExtensionBuilder((
                (FluentToken token, LexemeAttribute attribute, GenericLexer<FluentToken> genericLexer) =>
                {
                    var fsmBuilder = genericLexer.FSMBuilder;
                    NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) => 
                    {
                        match.Properties[GenericLexer<FluentToken>.DerivedToken] = FluentToken.THREE_DOT;
                        return match;
                    };
                    
                    fsmBuilder.GoTo(GenericLexer<FluentToken>.start)
                        .ConstantTransition("...")
                        .End(GenericToken.Extension) // mark as ending node 
                        .CallBack(callback); 
                }))
            
            .Build("en");
        Check.That(lexer).IsOk();
        
        var tokenized = lexer.Result.Tokenize(@"
2024-12-20
extension
...
");
        Check.That(tokenized).IsOkLexing();
        var all = tokenized.Tokens.AllExceptWhiteSpaces;
        Check.That(all).CountIs(4);
        Check.That(all.Extracting(x => x.TokenID).Take(3)).IsEqualTo(new List<FluentToken>()
        {
            FluentToken.DATE,
            FluentToken.ID,
            FluentToken.THREE_DOT
        });
        Check.That(all[2].Value).IsEqualTo("...");
    }
    
    
    [Fact]
    public void TestFluentLexerBuilderWitTokenCallbacks()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Date(FluentToken.DATE,DateFormat.YYYYMMDD,'-')
            .AlphaNumDashId(FluentToken.ID)
            .Keyword(FluentToken.HELLO, "hello")
            .Keyword(FluentToken.WORLD, "world")
            .UseTokenCallback(FluentToken.WORLD, token =>
            {
                token.TokenID = FluentToken.HELLO;
                return token;
            })
            .UseTokenCallback(FluentToken.HELLO, token => 
            {
                token.TokenID = FluentToken.WORLD;
                return token;
            })
            .Build("en");
        Check.That(lexer).IsOk();
        
        var tokenized = lexer.Result.Tokenize(@"
2024-12-20
post-process
hello 
world
");
        Check.That(tokenized).IsOkLexing();
        var all = tokenized.Tokens.AllExceptWhiteSpaces;
        Check.That(all).CountIs(5);
        Check.That(all.Extracting(x => x.TokenID).Take(4)).IsEqualTo(new List<FluentToken>()
        {
            FluentToken.DATE,
            FluentToken.ID,
            FluentToken.WORLD,
            FluentToken.HELLO,
        });
        
    }
}