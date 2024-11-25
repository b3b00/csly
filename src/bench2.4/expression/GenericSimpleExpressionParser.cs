using sly.lexer;
using sly.parser.generator;

namespace bench.simpleExpressionParser;

[ParserRoot("root")]
//[BroadenTokenWindow]
public class GenericSimpleExpressionParser
{

    [Production("root : GenericSimpleExpressionParser_expressions")]
        
    public double Root(double value) => value;
        
    [Operation((int) GenericExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
    [Operation("MINUS", Affix.InFix, Associativity.Left, 10)]
    public double BinaryTermExpression(double left, Token<GenericExpressionToken> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case GenericExpressionToken.PLUS:
            {
                result = left + right;
                break;
            }
            case GenericExpressionToken.MINUS:
            {
                result = left - right;
                break;
            }
        }

        return result;
    }


    [Operation((int) GenericExpressionToken.TIMES, Affix.InFix, Associativity.Right, 50)]
    [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
    [NodeName("multiplication_or_division")]
    public double BinaryFactorExpression(double left, Token<GenericExpressionToken> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case GenericExpressionToken.TIMES:
            {
                result = left * right;
                break;
            }
            case GenericExpressionToken.DIVIDE:
            {
                result = left / right;
                break;
            }
        }

        return result;
    }


    [Prefix((int) GenericExpressionToken.MINUS,  Associativity.Right, 100)]
    public double PreFixExpression(Token<GenericExpressionToken> operation, double value)
    {
        return -value;
    }

    [Postfix((int) GenericExpressionToken.FACTORIAL, Associativity.Right, 100)]
    public double PostFixExpression(double value, Token<GenericExpressionToken> operation)
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
    public double OperandDouble(Token<GenericExpressionToken> value)
    {
        return value.DoubleValue;
    }
        
    [Production("primary_value : INT")]
    [NodeName("integer")]
    public double OperandInt(Token<GenericExpressionToken> value)
    {
        return value.DoubleValue;
    }

    [Production("primary_value : LPAREN GenericSimpleExpressionParser_expressions RPAREN")]
    [NodeName("group")]
    public double OperandParens(Token<GenericExpressionToken> lparen, double value, Token<GenericExpressionToken> rparen)
    {
        return value;
    }
}