using System.Collections.Generic;
using System.Linq;
using aot.parser;
using csly.indentedWhileLang.parser;
using NFluent;
using ParserTests.aot.expressions;
using sly.lexer;
using Xunit;

namespace ParserTests.aot;

public class AotTests
{

    private IAotLexerBuilder<AotExpressionsLexer> BuildAotexpressionLexer()
    {
        var builder = AotLexerBuilder<AotExpressionsLexer>.NewBuilder<AotExpressionsLexer>();
        var lexerBuilder = builder.Double(AotExpressionsLexer.DOUBLE)
            .Sugar(AotExpressionsLexer.PLUS, "+")
            .Keyword(AotExpressionsLexer.PLUS, "PLUS")
            .Labeled("en", "sum")
            .Labeled("fr", "somme")
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
        var lexer = lexerBuilder.Build();
        Check.That(lexer).IsNotNull();
        var lexed = lexer.Tokenize("1 + 1");
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
        
        var builder = AotEBNFParserBuilder<AotExpressionsLexer,double>.NewBuilder<AotExpressionsLexer,double > (expressionsParserInstance,"root");

        var lexerBuilder = BuildAotexpressionLexer();

        var p = builder.UseMemoization()
            .WithLexerbuilder(lexerBuilder)
            .Production("root : AotParser_expressions", (args) =>
            {
                var result = expressionsParserInstance.Root((double)args[0]);
                return result;
            })
            .Right(10, AotExpressionsLexer.PLUS, (args =>
            {
                double result = expressionsParserInstance.BinaryTermExpression((double)args[0], (Token<AotExpressionsLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(10, AotExpressionsLexer.MINUS, (args =>
            {
                double result = expressionsParserInstance.BinaryTermExpression((double)args[1], (Token<AotExpressionsLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Right(50, AotExpressionsLexer.TIMES, (args =>
            {
                double result =
                    expressionsParserInstance.BinaryFactorExpression((double)args[0], (Token<AotExpressionsLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(50, AotExpressionsLexer.DIVIDE, (args =>
            {
                double result =
                    expressionsParserInstance.BinaryFactorExpression((double)args[1], (Token<AotExpressionsLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Prefix(100, AotExpressionsLexer.MINUS, (object[] args) =>
            {
                return expressionsParserInstance.PreFixExpression((Token<AotExpressionsLexer>)args[0], (double)args[1]);
            })
            .Postfix(100, "'!'", (object[] args) =>
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
        var r = parser.Parse(" 2 + 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(4);
        r = parser.Parse(" 2 + 2 * 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(6);
    }


    private IAotLexerBuilder<IndentedWhileTokenGeneric> BuildAotWhileLexer()
    {
        var builder = AotLexerBuilder<IndentedWhileTokenGeneric>.NewBuilder<IndentedWhileTokenGeneric>();
        builder.IsIndentationAware()
            .UseIndentations("\t")
            // keywords
            .Keyword(IndentedWhileTokenGeneric.IF, "if")
            .Keyword(IndentedWhileTokenGeneric.THEN, "then")
            .Keyword(IndentedWhileTokenGeneric.ELSE, "else")
            .Keyword(IndentedWhileTokenGeneric.WHILE, "while")
            .Keyword(IndentedWhileTokenGeneric.DO, "do")
            .Keyword(IndentedWhileTokenGeneric.SKIP, "skip")
            .Keyword(IndentedWhileTokenGeneric.PRINT, "print")
            .Keyword(IndentedWhileTokenGeneric.TRUE, "true")
            .Keyword(IndentedWhileTokenGeneric.FALSE, "false")
            .Keyword(IndentedWhileTokenGeneric.NOT, "not")
            .Keyword(IndentedWhileTokenGeneric.AND, "and")
            .Keyword(IndentedWhileTokenGeneric.OR, "or")
            .Keyword(IndentedWhileTokenGeneric.RETURN, "return")
            // literals
            .AlphaNumDashId(IndentedWhileTokenGeneric.IDENTIFIER)
            .String(IndentedWhileTokenGeneric.STRING)
            .Integer(IndentedWhileTokenGeneric.INT)
            // operators
            .Sugar(IndentedWhileTokenGeneric.GREATER, ">")
            .Sugar(IndentedWhileTokenGeneric.LESSER, "<")
            .Sugar(IndentedWhileTokenGeneric.EQUALS, "==")
            .Sugar(IndentedWhileTokenGeneric.DIFFERENT, "!==")
            .Sugar(IndentedWhileTokenGeneric.CONCAT, ".")
            .Sugar(IndentedWhileTokenGeneric.ASSIGN, ":=")
            .Sugar(IndentedWhileTokenGeneric.PLUS, "+")
            .Sugar(IndentedWhileTokenGeneric.MINUS, "-")
            .Sugar(IndentedWhileTokenGeneric.TIMES, "*")
            .Sugar(IndentedWhileTokenGeneric.DIVIDE, "/")
            .Sugar(IndentedWhileTokenGeneric.SEMICOLON, ";")
            .SingleLineComment(IndentedWhileTokenGeneric.COMMENT, "#");
        return builder;
            
            
        return builder;
    }
    
    [Fact]
    public void AotWhileLexerTest()
    {
        var lexerBuilder = BuildAotWhileLexer();
        var lexer = lexerBuilder.Build();
        Check.That(lexer).IsNotNull();
        var commentResult = lexer.Tokenize("# comment");
        Check.That(commentResult).IsOkLexing();
        var commentChannel = commentResult.Tokens.GetChannel(Channels.Comments);
        Check.That(commentChannel.Tokens).CountIs(1);
        Check.That(commentChannel.Tokens[0].TokenID).IsEqualTo(IndentedWhileTokenGeneric.COMMENT);
        Check.That(commentChannel.Tokens[0].Value).IsEqualTo(" comment");
        // TODO AOT : check has COMMENT
        string program = @"
a:=0 
while a < 10 do 
    print a
    a := a +1
";
        var programResult = lexer.Tokenize(program);
        Check.That(programResult).IsOkLexing();
        Check.That(programResult.Tokens.MainTokens().Exists(x => x.IsIndent)).IsTrue();
        Check.That(programResult.Tokens.MainTokens().Exists(x => x.IsUnIndent)).IsTrue();
        // TODO AOT : check has indents
    }
    
    
}