using System.Collections.Generic;
using System.Linq;
using aot.parser;
using NFluent;
using sly.lexer;
using Xunit;

namespace ParserTests.aot;

public class AotTests
{

    private IAotLexerBuilder<AotLexer> BuildLexer()
    {
        var builder = AotLexerBuilder<AotLexer>.NewBuilder<AotLexer>();
        var lexerBuilder = builder.Double(AotLexer.DOUBLE)
            .Sugar(AotLexer.PLUS, "+")
            .Keyword(AotLexer.PLUS, "PLUS")
            .Labeled("en", "sum")
            .Labeled("fr", "somme")
            .Sugar(AotLexer.MINUS, "-")
            .Sugar(AotLexer.TIMES, "*")
            .Sugar(AotLexer.DIVIDE, "/")
            .Sugar(AotLexer.LPAREN, "(")
            .Sugar(AotLexer.RPAREN, ")")
            .Sugar(AotLexer.FACTORIAL, "!")
            .Sugar(AotLexer.INCREMENT, "++")
            .AlphaNumId(AotLexer.IDENTIFIER);
            
        return lexerBuilder;
    }
    
    [Fact]
    public void AotLexerTest()
    {
        var lexerBuilder = BuildLexer();
        var lexer = lexerBuilder.Build();
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

    [Fact]
    public void AotParserTest()
    {
         AotParser parserInstance = new AotParser();
        
        var builder = AotEBNFParserBuilder<AotLexer,double>.NewBuilder<AotLexer,double > (parserInstance,"root");

        var lexerBuilder = BuildLexer();

        var p = builder.UseMemoization()
            .WithLexerbuilder(lexerBuilder)
            .Production("root : AotParser_expressions", (args) =>
            {
                var result = parserInstance.Root((double)args[0]);
                return result;
            })
            .Right(10, AotLexer.PLUS, (args =>
            {
                double result = parserInstance.BinaryTermExpression((double)args[0], (Token<AotLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(10, AotLexer.MINUS, (args =>
            {
                double result = parserInstance.BinaryTermExpression((double)args[1], (Token<AotLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Right(50, AotLexer.TIMES, (args =>
            {
                double result =
                    parserInstance.BinaryFactorExpression((double)args[0], (Token<AotLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(50, AotLexer.DIVIDE, (args =>
            {
                double result =
                    parserInstance.BinaryFactorExpression((double)args[1], (Token<AotLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Prefix(100, AotLexer.MINUS, (object[] args) =>
            {
                return parserInstance.PreFixExpression((Token<AotLexer>)args[0], (double)args[1]);
            })
            .Postfix(100, "'!'", (object[] args) =>
            {
                return parserInstance.PostFixExpression((double)args[0], (Token<AotLexer>)args[1]);
            })
            // .Operand("operand : primary_value", args =>
            // {
            //     return parserInstance.OperandValue((double)args[0]);
            // })
            .Operand("primary_value : DOUBLE", args =>
            {
                return parserInstance.OperandDouble((Token<AotLexer>)args[0]);
            })
            .Operand("primary_value : INT", args =>
            {
                return parserInstance.OperandInt((Token<AotLexer>)args[0]);
            })
            .Operand("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN", args =>
            {
                return parserInstance.OperandGroup((Token<AotLexer>)args[0], (double)args[1], (Token<AotLexer>)args[2]);
            });

        var parser = p.BuildParser();
        var r = parser.Parse(" 2 + 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(4);
        r = parser.Parse(" 2 + 2 * 2");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo(6);
    }
}