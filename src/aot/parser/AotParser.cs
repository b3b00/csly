using aot.lexer;
using sly.lexer;
using sly.parser.generator;

namespace aot.parser;

[ParserRoot("root")]
public class AotParser
{
    [Production("root : SimpleExpressionParser_expressions")]
    public double Root(double value) => value;
        
    [Operation((int) AotLexer.PLUS, Affix.InFix, Associativity.Right, 10)]
    [Operation("MINUS", Affix.InFix, Associativity.Left, 10)]
    public double BinaryTermExpression(double left, Token<AotLexer> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case AotLexer.PLUS:
            {
                result = left + right;
                break;
            }
            case AotLexer.MINUS:
            {
                result = left - right;
                break;
            }
        }

        return result;
    }


    [Operation((int) AotLexer.TIMES, Affix.InFix, Associativity.Right, 50)]
    [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
    [NodeName("multiplication_or_division")]
    public double BinaryFactorExpression(double left, Token<AotLexer> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case AotLexer.TIMES:
            {
                result = left * right;
                break;
            }
            case AotLexer.DIVIDE:
            {
                result = left / right;
                break;
            }
        }

        return result;
    }


    [Prefix((int) AotLexer.MINUS,  Associativity.Right, 100)]
    public double PreFixExpression(Token<AotLexer> operation, double value)
    {
        return -value;
    }

    [Postfix((int) AotLexer.FACTORIAL, Associativity.Right, 100)]
    public double PostFixExpression(double value, Token<AotLexer> operation)
    {
        if (operation.TokenID == AotLexer.SQUARE)
        {
            return value * value;
        }
        if(operation.TokenID == AotLexer.FACTORIAL || operation.Value == "!")
        {
            var factorial = 1;
            for (var i = 1; i <= value; i++) factorial *= i;
            return factorial;
        }
        return value;
    }

    [Operand]
    [Production("operand : primary_value")]
    [NodeName("double")]
    public double OperandValue(double value)
    {
        return value;
    }


    [Production("primary_value : DOUBLE")]
    [NodeName("double")]
    public double OperandDouble(Token<AotLexer> value)
    {
        return value.DoubleValue;
    }
        
    [Production("primary_value : INT")]
    [NodeName("integer")]
    public double OperandInt(Token<AotLexer> value)
    {
        return value.DoubleValue;
    }

    [Production("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN")]
    [NodeName("group")]
    public double OperandGroup(Token<AotLexer> lparen, double value, Token<AotLexer> rparen)
    {
        return value;
    }
}