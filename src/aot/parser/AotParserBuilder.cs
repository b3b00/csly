using aot.lexer;
using sly.lexer;
using sly.parser;

namespace aot.parser;

public class AotParserBuilder
{
    public ISyntaxParser<AotLexer, double> FluentInitializeCenericLexer()
    {
        AotParser parser = new AotParser();
        
        var builder = ParserBuilder<AotLexer,double>.NewBuilder<AotLexer,double > ();
        
        
        
        
        var p = builder.Production("root : SimpleExpressionParser_expressions", (args) =>
            {
                var result = parser.Root((double)args[0]);
                return result;
            })
            .Right(10,AotLexer.PLUS, (args =>
            {
                double result = parser.BinaryTermExpression((double)args[0], (Token<AotLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(10,AotLexer.MINUS, (args =>
            {
                double result = parser.BinaryTermExpression((double)args[1], (Token<AotLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Right(50,AotLexer.TIMES, (args =>
            {
                double result = parser.BinaryFactorExpression((double)args[0], (Token<AotLexer>)args[1], (double)args[2]);
                return result;
            }))
            .Right(50, AotLexer.DIVIDE, (args =>
            {
                double result = parser.BinaryFactorExpression((double)args[1], (Token<AotLexer>)args[2], (double)args[3]);
                return result;
            }))
            .Prefix(100,AotLexer.MINUS, (object[] args) =>
            {
                return parser.PreFixExpression((Token<AotLexer>)args[0], (double)args[1]);
            })
            .Postfix(100,AotLexer.FACTORIAL, (object[] args) =>
            {
                return parser.PostFixExpression((double)args[0], (Token<AotLexer>)args[1]);
            })
            .Operand("operand : primary_value", args =>
            {
                return parser.OperandValue((double)args[0]);
            })
            .Production("primary_value : DOUBLE", args =>
            {
                return parser.OperandDouble((Token<AotLexer>)args[0]);
            })
            .Production("primary_value : INT", args =>
            {
                return parser.OperandInt((Token<AotLexer>)args[0]);
            })
            .Production("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN", args =>
            {
                return parser.OperandGroup((Token<AotLexer>)args[0], (double)args[1], (Token<AotLexer>)args[2]);
            })
            .Build();
        return p;
    }
}