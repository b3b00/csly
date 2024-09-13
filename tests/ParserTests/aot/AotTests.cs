using System.Collections.Generic;
using System.Linq;
using aot.parser;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using NFluent;
using ParserTests.aot.expressions;
using Sigil;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using XML;
using Xunit;

namespace ParserTests.aot;

public enum TestLexer
{
    Eos,
    Value1,
    Value2,
    Ws,
    Eol,
}

public class AotTests
{
    private readonly AotIndentedWhileParserBuilder _aotIndentedWhileParserBuilder = new AotIndentedWhileParserBuilder();

    [Fact]
    public void CannotMixRegexAndGGenericTest()
    {
        var builder = AotLexerBuilder<TestLexer>.NewBuilder();
        var mixedLexer = builder.Double(TestLexer.Value1)
            .Regex(TestLexer.Value2, "a*")
            .Build("en");
        Check.That(mixedLexer).Not.IsOk();
        Check.That(mixedLexer.Errors.Exists(x => x.Code == ErrorCodes.LEXER_CANNOT_MIX_GENERIC_AND_REGEX));
    }
    
    [Fact]
    public void ValueRegexLexerTest()
    {
        var builder = AotLexerBuilder<TestLexer>.NewBuilder();
        var validLexerResult = builder.Regex(TestLexer.Value1,"[0-9]+")
            .Regex(TestLexer.Value2, "([a-z]|[A-Z])+")
            .Regex(TestLexer.Eol,"[\\r|\\n]+",true,true)
            .Regex(TestLexer.Ws,"[ |\\t]+",true)
            .Build("en");
        Check.That(validLexerResult).IsOk();
        var lexer = validLexerResult.Result;
        var result = lexer.Tokenize("42 abc");
        Check.That(result).IsOkLexing();
        var tokens = result.Tokens.MainTokens();
        Check.That(tokens).CountIs(3);
        Check.That(tokens[2].IsEOS).IsTrue();
        Check.That(tokens.Take(2).Extracting(x => x.TokenID))
            .IsEqualTo(new [] { TestLexer.Value1, TestLexer.Value2});
        Check.That(tokens.Take(2).Extracting(x => x.Value))
            .IsEqualTo(new [] { "42", "abc" });

    }
    
    private IAotLexerBuilder<AotExpressionsLexer> BuildAotexpressionLexer()
    {
        var builder = AotLexerBuilder<AotExpressionsLexer>.NewBuilder();
        var lexerBuilder = builder.Double(AotExpressionsLexer.DOUBLE)
            .Sugar(AotExpressionsLexer.PLUS, "+")
            .Keyword(AotExpressionsLexer.PLUS, "PLUS")
            .WithLabel("fr","addition")
            .WithLabel("en","sum")
            .Sugar(AotExpressionsLexer.MINUS, "-")
            .Sugar(AotExpressionsLexer.TIMES, "*")
            .Sugar(AotExpressionsLexer.DIVIDE, "/")
            .Sugar(AotExpressionsLexer.LPAREN, "(")
            .Sugar(AotExpressionsLexer.RPAREN, ")")
            .Sugar(AotExpressionsLexer.FACTORIAL, "!")
            .Sugar(AotExpressionsLexer.INCREMENT, "++")
            .AlphaNumId(AotExpressionsLexer.IDENTIFIER);
            
        return lexerBuilder;
    }
    
    [Fact]
    public void AotExpressionLexerTest()
    {
        var lexerBuilder = BuildAotexpressionLexer();
        var lexer = lexerBuilder.Build("en");
        Check.That(lexer).IsNotNull();
        var lexed = lexer.Result.Tokenize("2+2");
        Check.That(lexed).IsOkLexing();
        Check.That(lexed.Tokens.MainTokens()).CountIs(4);
        Check.That(lexed.Tokens.MainTokens().Take(3).Extracting(x => x.TokenID)).IsEqualTo(new[]
        {
            AotExpressionsLexer.DOUBLE,
            AotExpressionsLexer.PLUS,
            AotExpressionsLexer.DOUBLE
        });
    }

    [Fact]
    public void AotExpressionParserTest()
    {
         AotExpressionsParser expressionsParserInstance = new AotExpressionsParser();
        
        var builder = AotEBNFParserBuilder<AotExpressionsLexer,double>.NewBuilder(expressionsParserInstance,"root");

        var lexerBuilder = BuildAotexpressionLexer();

        var p = builder.UseMemoization()
            .WithLexerbuilder(lexerBuilder)
            .Production("root : AotExpressionsParser_expressions", (args) =>
            {
                var result = expressionsParserInstance.Root((double)args[0]);
                return result;
            })
            .Right(AotExpressionsLexer.PLUS, 10, (args =>
            {
                double result = expressionsParserInstance.BinaryTermExpression((double)args[0], (Token<AotExpressionsLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(AotExpressionsLexer.MINUS, 10, (args =>
            {
                double result = expressionsParserInstance.BinaryTermExpression((double)args[1], (Token<AotExpressionsLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Right(AotExpressionsLexer.TIMES, 50, (args =>
            {
                double result =
                    expressionsParserInstance.BinaryFactorExpression((double)args[0], (Token<AotExpressionsLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(AotExpressionsLexer.DIVIDE, 50, (args =>
            {
                double result =
                    expressionsParserInstance.BinaryFactorExpression((double)args[1], (Token<AotExpressionsLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Prefix(AotExpressionsLexer.MINUS, 100, (object[] args) =>
            {
                return expressionsParserInstance.PreFixExpression((Token<AotExpressionsLexer>)args[0], (double)args[1]);
            })
            .Postfix("'!'", 100, (object[] args) =>
            {
                return expressionsParserInstance.PostFixExpression((double)args[0], (Token<AotExpressionsLexer>)args[1]);
            })
            // .Operand("operand : primary_value", args =>
            // {
            //     return parserInstance.OperandValue((double)args[0]);
            // })
            .Operand("primary_value : DOUBLE", args =>
            {
                return expressionsParserInstance.OperandDouble((Token<AotExpressionsLexer>)args[0]);
            })
            .Operand("primary_value : INT", args =>
            {
                return expressionsParserInstance.OperandInt((Token<AotExpressionsLexer>)args[0]);
            })
            .Operand("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN", args =>
            {
                return expressionsParserInstance.OperandGroup((Token<AotExpressionsLexer>)args[0], (double)args[1], (Token<AotExpressionsLexer>)args[2]);
            });

        var parser = p.BuildParser();
        Check.That(parser).IsOk();
        var r = parser.Result.Parse(" 2 + 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(4);
        r = parser.Result.Parse(" 2 + 2 * 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(6);
    }


    [Fact]
    public void AotWhileLexerTest()
    {
        var lexerBuilder = _aotIndentedWhileParserBuilder.BuildAotWhileLexer();
        var lexer = lexerBuilder.Build("en");
        Check.That(lexer).IsNotNull();
        var commentResult = lexer.Result.Tokenize("# comment");
        Check.That(commentResult).IsOkLexing();
        var commentChannel = commentResult.Tokens.GetChannel(Channels.Comments);
        Check.That(commentChannel.Tokens).CountIs(1);
        Check.That(commentChannel.Tokens[0].TokenID).IsEqualTo(IndentedWhileTokenGeneric.COMMENT);
        Check.That(commentChannel.Tokens[0].Value).IsEqualTo(" comment");
        string program = @"
a:=0 
while a < 10 do 
	print a
	a := a +1
";
        var programResult = lexer.Result.Tokenize(program);
        Check.That(programResult).IsOkLexing();
        Check.That(programResult.Tokens.MainTokens().Exists(x => x.IsIndent)).IsTrue();
        Check.That(programResult.Tokens.MainTokens().Exists(x => x.IsUnIndent)).IsTrue();
        
    }

    [Fact]
    public void AotWhileParserTest()
    {
        var builder = _aotIndentedWhileParserBuilder.BuildAotWhileParser();
        var buildResult = builder.BuildParser();
        Check.That(buildResult).IsOk();
        var parser = buildResult.Result;
        Check.That(parser).IsNotNull();
        var program = @"
# infinite loop
while true do
    skip
";
        var result = parser.Parse(program);
        Check.That(result).IsOkParsing();

        Check.That(result.Result).IsInstanceOf<SequenceStatement>();
        var seq = result.Result as SequenceStatement;
        Check.That(seq.Get(0)).IsInstanceOf<WhileStatement>();
        var whil = seq.Get(0) as WhileStatement;
        var cond = whil.Condition;
        Check.That(cond).IsInstanceOf<BoolConstant>();
        Check.That((cond as BoolConstant).Value).IsTrue();
        var s = whil.BlockStmt;
        Check.That(whil.BlockStmt).IsInstanceOf<SequenceStatement>();
        var seqBlock = whil.BlockStmt as SequenceStatement;
        Check.That(seqBlock).CountIs(1);
        Check.That(seqBlock.Get(0)).IsInstanceOf<SkipStatement>();
    }


    [Fact]
    public void AotLexerCallbacksTest()
    {
        var builder = AotLexerBuilder<TestLexer>.NewBuilder();
        var lexerResult = builder.AlphaId(TestLexer.Value1)
            .UseTokenCallback(TestLexer.Value1, t =>
            {
                if (char.IsUpper(t.Value[0]))
                {
                    t.TokenID = TestLexer.Value2;
                }

                return t;
            })
            .Build("en");
        Check.That(lexerResult).IsOk();
        string source = "abc Abc";
        var result = lexerResult.Result.Tokenize(source);
        Check.That(result).IsOkLexing();
        var tokens = result.Tokens.MainTokens();
        Check.That(tokens).CountIs(3);
        Check.That(tokens.Take(2).Extracting(x => x.TokenID)).IsEqualTo(new[] { TestLexer.Value1, TestLexer.Value2 });
    }
    
    [Fact]
    public void AotLexerPostProcessTest()
    {
        var builder = AotLexerBuilder<TestLexer>.NewBuilder();
        var lexerBuilder = builder.AlphaId(TestLexer.Value1)
                .WithLabel("en","this is Value One")
            .UseLexerPostProcessor((List<Token<TestLexer>> tokens) =>
            {
                var processed = tokens.Select(x =>
                {
                    if (x.TokenID == TestLexer.Value1 && char.IsUpper(x.Value[0]))
                    {
                        x.TokenID = TestLexer.Value2;
                    }

                    return x;
                }).ToList();
                return processed;
            });

        var parserResult = AotEBNFParserBuilder<TestLexer, string>.NewBuilder( "root")
            .Production("root : [Value1|Value2]*", args =>
            {
                var t = args[0] as List<Token<TestLexer>>;
                return string.Join(",", t.Select(x => x.TokenID));
            })
            .WithLexerbuilder(lexerBuilder)
            .BuildParser();
            
            
        Check.That(parserResult).IsOk();
        string source = "abc Abc";
        var result = parserResult.Result.Parse(source);
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("Value1,Value2");
    }
    
    [Fact]
    public void AotLexerLabelsTest()
    {
        var builder = AotLexerBuilder<TestLexer>.NewBuilder();
        var lexerBuilder = builder.AlphaId(TestLexer.Value1)
            .WithLabel("en", "Value One")
            .WithLabel("fr", "Valeur un")
            .Sugar(TestLexer.Value2, "*-*")
            .WithLabel("en","star-dash-star")
            .WithLabel("fr","étoile-tiret-étoile");
            
            

        var instance = "no instance";
        var englishParserResult = AotEBNFParserBuilder<TestLexer, string>.NewBuilder(instance, "root","en")
            .Production("root : [Value1|Value2]*", args =>
            {
                var t = args[0] as List<Token<TestLexer>>;
                return string.Join(",", t.Select(x => x.TokenID+"-"+x.Label));
            })
            .WithLexerbuilder(lexerBuilder)
            .BuildParser();
            
            
        Check.That(englishParserResult).IsOk();
        string source = "abc *-*";
        var result = englishParserResult.Result.Parse(source);
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("Value1-Value One,Value2-star-dash-star");
        
        var frenchParserResult = AotEBNFParserBuilder<TestLexer, string>.NewBuilder(instance, "root","fr")
            .Production("root : [Value1|Value2]*", args =>
            {
                var t = args[0] as List<Token<TestLexer>>;
                return string.Join(",", t.Select(x => x.TokenID+"-"+x.Label));
            })
            .WithLexerbuilder(lexerBuilder)
            .BuildParser();
            
            
        Check.That(frenchParserResult).IsOk();
        result = frenchParserResult.Result.Parse(source);
        Check.That(result).IsOkParsing();
        Check.That(result.Result).IsEqualTo("Value1-Valeur un,Value2-étoile-tiret-étoile");
        
    }

    [Fact]
    public void AotLexerModesTest()
    {
        var builder = AotLexerBuilder<MinimalXmlLexer>.NewBuilder();
        var lexerBuilder = builder.Sugar(MinimalXmlLexer.OPEN, "<")
            .Push(MinimalXmlLexer.OPEN, "tag")
            .UpTo(MinimalXmlLexer.CONTENT, "<")
            .Sugar(MinimalXmlLexer.OPEN_PI, "<?")
            .Push(MinimalXmlLexer.OPEN_PI, "pi")
            .MultiLineComment(MinimalXmlLexer.COMMENT, "<!--", "-->", false, Channels.Main)
            .AlphaId(MinimalXmlLexer.ID).WithModes("pi", "tag")
            .Sugar(MinimalXmlLexer.SLASH, "/").WithModes( "tag")
            .Sugar(MinimalXmlLexer.EQUALS, "=").WithModes( "tag")
            .String(MinimalXmlLexer.VALUE).WithModes(  "pi", "tag" )
            .Sugar(MinimalXmlLexer.CLOSE_PI, "?>").WithModes( "pi")
            .Sugar(MinimalXmlLexer.CLOSE, ">").WithModes( "tag")
            .Pop(MinimalXmlLexer.CLOSE)
            .Pop(MinimalXmlLexer.CLOSE_PI);
        var lexerResult = lexerBuilder.Build("en");
        Check.That(lexerResult).IsOk();
        string xml = @"hello
<tag attr=""value"">inner text</tag>
<!-- this is a comment -->
<? PI attr=""test""?>";
        var result = lexerResult.Result.Tokenize(xml);
        Check.That(result.IsOk).IsTrue();
        var expectedTokens = new List<MinimalXmlLexer>()
        {
            MinimalXmlLexer.CONTENT,
            MinimalXmlLexer.OPEN,
            MinimalXmlLexer.ID,
            MinimalXmlLexer.ID,
            MinimalXmlLexer.EQUALS,
            MinimalXmlLexer.VALUE,
            MinimalXmlLexer.CLOSE,
            MinimalXmlLexer.CONTENT,
            MinimalXmlLexer.OPEN,
            MinimalXmlLexer.SLASH,
            MinimalXmlLexer.ID,
            MinimalXmlLexer.CLOSE,
            MinimalXmlLexer.COMMENT,
            MinimalXmlLexer.OPEN_PI,
            MinimalXmlLexer.ID,
            MinimalXmlLexer.ID,
            MinimalXmlLexer.EQUALS,
            MinimalXmlLexer.VALUE,
            MinimalXmlLexer.CLOSE_PI
        };
        var tokens = result.Tokens.MainTokens();
        Check.That(expectedTokens).CountIs(tokens.Count-1);

        Check.That(tokens.Extracting("TokenID")).Contains(expectedTokens);





    }

}