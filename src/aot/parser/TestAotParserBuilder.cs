using aot.lexer;
using sly.buildresult;
using sly.lexer;
using sly.parser;

namespace aot.parser;

public class TestAotParserBuilder
{
    public BuildResult<Parser<AotLexer, double>> FluentInitializeCenericLexer()
    {
        AotParser parserInstance = new AotParser();
        
        var builder = AotEBNFParserBuilder<AotLexer,double>.NewBuilder(parserInstance,"root");

        var testLexerbuilder = new TestAotLexerBuilder();
        var lexerBuilder = testLexerbuilder.FluentInitializeCenericLexerForParserTest();

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
            .Postfix(100,AotLexer.SQUARE, (args) =>
            {
                return parserInstance.PostFixExpression((double)args[0], (Token<AotLexer>)args[1]);
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
        return parser;
    }
}