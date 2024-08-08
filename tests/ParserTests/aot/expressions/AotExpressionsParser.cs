using sly.lexer;
using sly.parser.generator;

namespace ParserTests.aot.expressions;

public class AotExpressionsParser
{
    [Production("root : SimpleExpressionParser_expressions")]
    public double Root(double value) => value;
        
    [Operation((int) AotExpressionsLexer.PLUS, Affix.InFix, Associativity.Right, 10)]
    [Operation("MINUS", Affix.InFix, Associativity.Left, 10)]
    public double BinaryTermExpression(double left, Token<AotExpressionsLexer> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case AotExpressionsLexer.PLUS:
            {
                result = left + right;
                break;
            }
            case AotExpressionsLexer.MINUS:
            {
                result = left - right;
                break;
            }
        }

        return result;
    }


    [Operation((int) AotExpressionsLexer.TIMES, Affix.InFix, Associativity.Right, 50)]
    [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
    [NodeName("multiplication_or_division")]
    public double BinaryFactorExpression(double left, Token<AotExpressionsLexer> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case AotExpressionsLexer.TIMES:
            {
                result = left * right;
                break;
            }
            case AotExpressionsLexer.DIVIDE:
            {
                result = left / right;
                break;
            }
        }

        return result;
    }


    [Prefix((int) AotExpressionsLexer.MINUS,  Associativity.Right, 100)]
    public double PreFixExpression(Token<AotExpressionsLexer> operation, double value)
    {
        return -value;
    }

    [Postfix((int) AotExpressionsLexer.FACTORIAL, Associativity.Right, 100)]
    public double PostFixExpression(double value, Token<AotExpressionsLexer> operation)
    {
        var factorial = 1;
        for (var i = 1; i <= value; i++) factorial = factorial * i;
        return factorial;
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
    public double OperandDouble(Token<AotExpressionsLexer> value)
    {
        return value.DoubleValue;
    }
        
    [Production("primary_value : INT")]
    [NodeName("integer")]
    public double OperandInt(Token<AotExpressionsLexer> value)
    {
        return value.DoubleValue;
    }

    [Production("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN")]
    [NodeName("group")]
    public double OperandGroup(Token<AotExpressionsLexer> lparen, double value, Token<AotExpressionsLexer> rparen)
    {
        return value;
    }
}