using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using simpleExpressionParser;
using sly.buildresult;
using sly.lexer;
using sly.lexer.fluent;
using sly.lexer.fsm;
using sly.parser;
using sly.parser.generator;
using sly.parser.parser;
using sly.parser.syntax.tree;
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
        var all = tokenized.Tokens.GetAllExceptWhiteSpaces();
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
        var all = tokenized.Tokens.GetAllExceptWhiteSpaces().ToList();
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
        var all = tokenized.Tokens.GetAllExceptWhiteSpaces();
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
            .Regex(FluentToken.THREE_DOT, "\\.{3}")
            .Regex(FluentToken.EOL, "[\\r\\n]*", true, true)
            .Build("en");
        Check.That(lexer).IsOk();
        var tokenized = lexer.Result.Tokenize(@"
hello
...
world
");
        Check.That(tokenized).IsOkLexing();
        var all = tokenized.Tokens.GetAllExceptWhiteSpaces();
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
    
    [Fact]
    public void TestFluentExpressionParserWithExplicits()
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
            .Left("'moins'", 10, (object[] args) =>
            {
                var l = (int)args[0];
                var r = (int)args[2];
                return l - r;
            })
            .Left("'plus'", 10, (object[] args) =>
            {
                var l = (int)args[0];
                var r = (int)args[2];
                return l + r;
            })
            .Right("'fois'", 50, (object[] args) =>
            {
                var l = (int)args[0];
                var r = (int)args[2];
                return l * r;
            })
            .Left("'div'", 50, (object[] args) =>
            {
                var l = (int)args[0];
                var r = (int)args[2];
                return l / r;
            })
            .Prefix("'moins'", 100, (object[] args) =>
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
        var result = parser.Parse("2 plus 2");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(2 + 2);
        result = parser.Parse("1 moins 2  moins 3");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(1 - 2 - 3);
        result = parser.Parse("1 plus 2 fois 3");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(1 + 2 * 3);
        result = parser.Parse("moins 1 plus 2 ");
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo(-1 + 2);
        
    }

    [Fact]
    public void TestFluentSubNodeNames()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Date(FluentToken.DATE, DateFormat.YYYYMMDD, '-')
            .AlphaNumDashId(FluentToken.ID)
            .UseLexerPostProcessor(tokens =>
            {
                return tokens.Select<Token<FluentToken>, Token<FluentToken>>(x =>
                {
                    if (x.TokenID == FluentToken.ID)
                    {
                        x.SpanValue = x.Value.ToUpper().AsMemory();
                    }

                    return x;
                }).ToList();
            });

        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : (l  'SEP'[d] r)*", args =>
            {
                var items = args[0] as List<Group<FluentToken, string>>;
                var r = string.Join(",", items.Select(x => $"{x.Value(0)}|{x.Value(1)}"));
                return r;

            }).WithSubNodeNamed("items")
            .Production("l : ID", args =>
            {
                return (args[0] as Token<FluentToken>)?.Value;
            }).Named("left")
            .Production("r : ID", args =>
            {
                return (args[0] as Token<FluentToken>)?.Value;
            }).Named("right")
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        var parsed = build.Result.Parse("a SEP b c SEP d ");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("A|B,C|D");
        var tree = parsed.SyntaxTree as SyntaxNode<FluentToken, string>;
        Check.That(tree).IsNotNull();
        Check.That(tree.Children).IsSingle();
        var child = tree.Children.FirstOrDefault();
        Check.That(child).IsNotNull();
        Check.That(child.Name).IsEqualTo("items");
        var childNode = child as SyntaxNode<FluentToken, string>;
        Check.That(childNode).IsNotNull();
        Check.That(childNode.Children).CountIs(2);
        var first = childNode.Children.FirstOrDefault() as SyntaxNode<FluentToken, string>;
        Check.That(first).IsNotNull();
        Check.That(first.Children).CountIs(3);
        var l = first.Children.FirstOrDefault() as SyntaxNode<FluentToken, string>;
        Check.That(l).IsNotNull();
        Check.That(l.Name).IsEqualTo("left");
    }
    
     [Fact]
    public void TestFluentBuildErrorMissingNonTerminal()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Date(FluentToken.DATE, DateFormat.YYYYMMDD, '-')
            .AlphaNumDashId(FluentToken.ID)
            .UseLexerPostProcessor(tokens =>
            {
                return tokens.Select<Token<FluentToken>, Token<FluentToken>>(x =>
                {
                    if (x.TokenID == FluentToken.ID)
                    {
                        x.SpanValue = x.Value.ToUpper().AsMemory();
                    }

                    return x;
                }).ToList();
            });
  
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : l r", args => "root").WithSubNodeNamed("root")
            .Production("r : ID", args => "right").Named("right")
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).Not.IsOk();
        Check.That(build.Errors.Extracting(x => x.Code)).Contains(ErrorCodes.PARSER_REFERENCE_NOT_FOUND);

    }
    
    [Fact]
    public void TestFluentBuildErrorLeftRecursion()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .Date(FluentToken.DATE, DateFormat.YYYYMMDD, '-')
            .AlphaNumDashId(FluentToken.ID)
            .UseLexerPostProcessor(tokens =>
            {
                return tokens.Select<Token<FluentToken>, Token<FluentToken>>(x =>
                {
                    if (x.TokenID == FluentToken.ID)
                    {
                        x.SpanValue = x.Value.ToUpper().AsMemory();
                    }

                    return x;
                }).ToList();
            });
  
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : l", args => "root").WithSubNodeNamed("root")
            .Production("l : l r ", args => "recurse").Named("recurse")
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).Not.IsOk();
        Check.That(build.Errors.Extracting(x => x.Code)).Contains(ErrorCodes.PARSER_LEFT_RECURSIVE);

    }

    [Fact]
    public void TestLexerErrorBadStringDelimiter()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumDashId(FluentToken.ID)
            .String(FluentToken.DATE, "x","y")
            .Int(FluentToken.DATE)
            .AlphaNumDashId(FluentToken.ID)
            .UseLexerPostProcessor(tokens =>
            {
                return tokens.Select<Token<FluentToken>, Token<FluentToken>>(x =>
                {
                    if (x.TokenID == FluentToken.ID)
                    {
                        x.SpanValue = x.Value.ToUpper().AsMemory();
                    }

                    return x;
                }).ToList();
            });
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : DATE", args => "root").WithSubNodeNamed("root")
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).Not.IsOk();
        Check.That(build.Errors.Extracting(x => x.Code))
            .Contains(ErrorCodes.LEXER_STRING_DELIMITER_CANNOT_BE_LETTER_OR_DIGIT);
    }
    
    [Fact]
    public void TestLexerErrorManySingleLineComment()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .Regex(FluentToken.START_ISLAND, "<")
            .AlphaNumId(FluentToken.ID);
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : DATE", args => "root").WithSubNodeNamed("root")
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).Not.IsOk();
        Check.That(build.Errors.Extracting(x => x.Code))
            .Contains(ErrorCodes.LEXER_CANNOT_MIX_GENERIC_AND_REGEX);
    }
    
    [Fact]
    public void TestRepetition()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumId(FluentToken.ID)
            .Int(FluentToken.INT);
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : ID{6} INT", args =>
            {
                List<Token<FluentToken>> items = (List<Token<FluentToken>>)args[0];
                return string.Join(",",items.Select(x => x.Value));
            }).WithSubNodeNamed("root")
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        Check.That(build.Result).IsNotNull();
        var parser = build.Result;
        var parsed = parser.Parse("a b c d e f  6");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,b,c,d,e,f");
        parsed = parser.Parse("a b c d e 5");
        Check.That(parsed).Not.IsOkParsing();
        var error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>;
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("5");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.INT);
        parsed = parser.Parse("a b c d e f g h i j k l 12");
        Check.That(parsed).Not.IsOkParsing();
        error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("g");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.ID);
        
        
        
        
    }
    
    [Fact]
    public void TestGroupRepeat()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumId(FluentToken.ID)
            .Int(FluentToken.INT);
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : (ID INT){6} INT", args =>
            {
                List<Group<FluentToken, string>> items = (List<Group<FluentToken, string>>)args[0];
                return string.Join(",",items.Select(x => x.Token(0).Value+"-"+x.Token(1).Value));
            }).WithSubNodeNamed("root")
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        Check.That(build.Result).IsNotNull();
        var parser = build.Result;
        var parsed = parser.Parse("a 1 b 2 c 3 d 4 e 5 f 6 999");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a-1,b-2,c-3,d-4,e-5,f-6");
        parsed = parser.Parse("a 1 b 2 c 3 d 4 e 5 0");
        Check.That(parsed).Not.IsOkParsing();
        var error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>;
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("0");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.INT);
        parsed = parser.Parse("a 1 b 2 c 3 d 4 e 5 f 6 g 7 h 8 i 9 j 10 k 11 l 12 8129");
        Check.That(parsed).Not.IsOkParsing();
        error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("g");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.ID);
    }
    
    [Fact]
    public void TestGroupRepetitionRange()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumId(FluentToken.ID)
            .Int(FluentToken.INT);
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : (ID INT){3-6} INT", args =>
            {
                List<Group<FluentToken, string>> items = (List<Group<FluentToken, string>>)args[0];
                return string.Join(",",items.Select(x => x.Token(0).Value+"-"+x.Token(1).Value));
            })
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        Check.That(build.Result).IsNotNull();
        var parser = build.Result;
        var parsed = parser.Parse("a 1 b 2 c 3 d 4 e 5 f 6 0");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a-1,b-2,c-3,d-4,e-5,f-6");
        parsed = parser.Parse("a 1 b 2 c 3 0");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a-1,b-2,c-3");
        parsed = parser.Parse("a 1 b 2 c 3 d 4 0");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a-1,b-2,c-3,d-4");
        
        
        parsed = parser.Parse("a 1 b 2 0");
        Check.That(parsed).Not.IsOkParsing();
        var error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("0");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.INT);
        parsed = parser.Parse("a 1 b 2 c 3 d 4 e 5 f 6 g 0");
        Check.That(parsed).Not.IsOkParsing();
        error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("g");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.ID);
        
        
        
        
    }
    
    [Fact]
    public void TestRepetitionRange()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumId(FluentToken.ID)
            .Int(FluentToken.INT);
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : ID{3-6} INT", args =>
            {
                List<Token<FluentToken>> items = (List<Token<FluentToken>>)args[0];
                return string.Join(",",items.Select(x => x.Value));
            })
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        Check.That(build.Result).IsNotNull();
        var parser = build.Result;
        var parsed = parser.Parse("a b c d e f 6 ");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,b,c,d,e,f");
        parsed = parser.Parse("a b c 3 ");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,b,c");
        parsed = parser.Parse("a b c d 4");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,b,c,d");
        
        
        parsed = parser.Parse("a b 2");
        Check.That(parsed).Not.IsOkParsing();
        var error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("2");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.INT);
        parsed = parser.Parse("a b c d e f g 7");
        Check.That(parsed).Not.IsOkParsing();
        error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("g");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.ID);
        
        
        
        
    }
    
    [Fact]
    public void TestRepetitionEmptyRange()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumId(FluentToken.ID)
            .Int(FluentToken.INT);
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : ID{0-6} INT", args =>
            {
                List<Token<FluentToken>> items = (List<Token<FluentToken>>)args[0];
                Token<FluentToken> n = (Token<FluentToken>)args[1];
                return string.Join(",",items.Select(x => x.Value))+ "#"+n.IntValue;
            })
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        Check.That(build.Result).IsNotNull();
        var parser = build.Result;
        var parsed = parser.Parse("a b c d e f 6 ");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,b,c,d,e,f#6");
        parsed = parser.Parse("a b c 3 ");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,b,c#3");
        parsed = parser.Parse("0");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("#0");
        parsed = parser.Parse("a b c d e f g 7");
        Check.That(parsed).Not.IsOkParsing();
        var error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("g");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.ID);
        Check.That(error.ExpectedTokens.Extracting(x => x.TokenId)).Contains(FluentToken.INT);




    }
    
    
     [Fact]
    public void TestChoiceRepeat()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumId(FluentToken.ID)
            .Sugar(FluentToken.COMMA,",")
            .Int(FluentToken.INT);
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : [ID|INT]{6} COMMA", args =>
            {
                List<Token<FluentToken>> items = (List<Token<FluentToken>>)args[0];
                return string.Join(",",items.Select(x => x.Value));
            }).WithSubNodeNamed("root")
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        Check.That(build.Result).IsNotNull();
        var parser = build.Result;
        var parsed = parser.Parse("a 1 b 2 c 3 ,");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,1,b,2,c,3");
        parsed = parser.Parse("a 1 b 2 c 3 d ,");
        Check.That(parsed).Not.IsOkParsing();
        var error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>;
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("d");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.ID);
    }
    
    [Fact]
    public void TestChoiceRepetitionRange()
    {
        var lexer = FluentLexerBuilder<FluentToken>.NewBuilder()
            .IgnoreEol(true)
            .IgnoreWhiteSpace(true)
            .IgnoreKeywordCase(true)
            .AlphaNumId(FluentToken.ID)
            .Int(FluentToken.INT)
            .Sugar(FluentToken.COMMA, ",")
            .Keyword(FluentToken.HELLO,"hello");
        
        var build = FluentEBNFParserBuilder<FluentToken, string>.NewBuilder(new FluentTests(), "root", "en")
            .Production("root : [ID|INT]{3-6} COMMA", args =>
            {
                List<Token<FluentToken>> items = (List<Token<FluentToken>>)args[0];
                return string.Join(",",items.Select(x => x.Value));
            })
            .WithLexerbuilder(lexer)
            .BuildParser();
        Check.That(build).IsOk();
        Check.That(build.Result).IsNotNull();
        var parser = build.Result;
        var parsed = parser.Parse("a 1 b 2 c 3 ,");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,1,b,2,c,3");
        parsed = parser.Parse("a 1 b  ,");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,1,b");
        parsed = parser.Parse("a 1 b 2 ,");
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("a,1,b,2");
        
        
        parsed = parser.Parse("a 1 b 2 hello");
        Check.That(parsed).Not.IsOkParsing();
        var error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("hello");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.HELLO);
        parsed = parser.Parse("a 1 b 2 c 3 d 4 e 5 f 6 g 0");
        Check.That(parsed).Not.IsOkParsing();
        error = parsed.Errors[0] as UnexpectedTokenSyntaxError<FluentToken>; 
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.UnexpectedToken.Value).IsEqualTo("d");
        Check.That(error.UnexpectedToken.TokenID).IsEqualTo(FluentToken.ID);
        
        
        
        
    }
}