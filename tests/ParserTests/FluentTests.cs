using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using simpleExpressionParser;
using sly.lexer;
using sly.lexer.fluent;
using sly.lexer.fsm;
using sly.parser.generator;
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
    EOL
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
            .Hexa(FluentToken.HEXA, "0x")
            .Date(FluentToken.DATE, DateFormat.YYYYMMDD, '-')
            .String(FluentToken.STRING)
            .MultiLineComment(FluentToken.COMMENT, "/*", "*/").OnChannel(Channels.Main)
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
            .Date(FluentToken.DATE, DateFormat.YYYYMMDD, '-')
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
    public void TestFluentLexerBuilderWithTokenCallbacks()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Date(FluentToken.DATE, DateFormat.YYYYMMDD, '-')
            .AlphaNumDashId(FluentToken.ID)
            .Keyword(FluentToken.HELLO, "hello")
            .Keyword(FluentToken.WORLD, "world")
            .WithCallback(FluentToken.WORLD, token =>
            {
                token.TokenID = FluentToken.HELLO;
                return token;
            })
            .WithCallback(FluentToken.HELLO, token =>
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

    [Fact]
    public void TestRegexFluentLexer()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .Regex(FluentToken.HELLO, "hello")
            .Regex(FluentToken.WORLD, "world")
            .Regex(FluentToken.THREE_DOT, ".{3}")
            .Regex(FluentToken.EOL, "[\\r\\n]*", true, true)
            .Build("en");
        Check.That(lexer).IsOk();
        var tokenized = lexer.Result.Tokenize(@"
hello
...
world
");
        Check.That(tokenized).IsOkLexing();
        var all = tokenized.Tokens.AllExceptWhiteSpaces;
        Check.That(all).CountIs(4);
        Check.That(all.Extracting(x => x.TokenID).Take(3)).IsEqualTo(new List<FluentToken>()
        {
            FluentToken.HELLO,
            FluentToken.THREE_DOT,
            FluentToken.WORLD
        });
    }

    [Fact]
    public void TestFluentParser()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Date(FluentToken.DATE, DateFormat.YYYYMMDD, '-')
            .AlphaNumDashId(FluentToken.ID)
            .Keyword(FluentToken.HELLO, "hello")
            .Keyword(FluentToken.WORLD, "world");


        var builder = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder("root", "en");
        var result = builder.Production("h : HELLO ID", (args) =>
            {
                var h = args[0] as Token<FluentToken>;
                var i = args[1] as Token<FluentToken>;
                return $"{h.Value}, {i.Value}";
            })
            .Production("w : WORLD DATE", args =>
            {
                var h = args[0] as Token<FluentToken>;
                var i = args[1] as Token<FluentToken>;
                return $"{h.Value}, {i.Value}";
            })
            .Production("item : h w", args =>
            {
                var items = args.Cast<string>().ToList();
                return $"hello({items[0]}) - world({items[1]})";
            })
            .Production("root : item*", args =>
            {
                var items = args[0] as List<string>;
                return string.Join("\n", items);
            })
            .WithLexerbuilder(lexer)
            .BuildParser();

        Check.That(result).IsOk();
        var parser = result.Result;
        var r = parser.Parse("hello olivier world 1977-03-30");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(@"hello(hello, olivier) - world(world, 1977-03-30)");
    }

    [Fact]
    public void TestFluentExpressionParser()
    {
        var lexer = FluentLexerBuilder<ExpressionToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .Int(ExpressionToken.INT)
            .Sugar(ExpressionToken.PLUS, "+")
            .Sugar(ExpressionToken.MINUS, "-")
            .Sugar(ExpressionToken.TIMES, "*")
            .Sugar(ExpressionToken.DIVIDE, "/");

        var build = FluentEBNFParserBuilder<ExpressionToken, int>.NewBuilder(new FluentTests(), "expression","en")
            .Production($"expression : {nameof(FluentTests)}_expressions", args =>
            {
                return (int)args[0];
            })
            .Left(ExpressionToken.MINUS, 10, (object[] args) =>
            {
                var l = (int)args[0];
                var r = (int)args[2];
                return l - r;
            })
            .Left(ExpressionToken.PLUS, 10, (object[] args) =>
            {
                var l = (int)args[0];
                var r = (int)args[2];
                return l + r;
            })
            .Right(ExpressionToken.TIMES, 50, (object[] args) =>
            {
                var l = (int)args[0];
                var r = (int)args[2];
                return l * r;
            })
            .Left(ExpressionToken.DIVIDE, 50, (object[] args) =>
            {
                var l = (int)args[0];
                var r = (int)args[2];
                return l / r;
            })
            .Prefix(ExpressionToken.MINUS, 100, (object[] args) =>
            {
                return -(int)args[1];
            })
            .Operand("operand : INT", args =>
            {
                var v = args[0] as Token<ExpressionToken>;
                return v.IntValue;
            })
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        var parser = build.Result;
        var result = parser.Parse("2 + 2");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(2 + 2);
        result = parser.Parse("1 - 2 -3");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(1 - 2 - 3);
        result = parser.Parse("1 + 2 * 3");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(1 + 2 * 3);
        result = parser.Parse("-1 + 2 ");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(-1 + 2);
        
    }
}