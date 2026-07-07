using csly.GeneratedExpressionParser.generatedgenericsimpleexpressionparser.models;



namespace generatedExpressions;

[ParserRoot("root")]
//[BroadenTokenWindow]
public class GeneratedGenericSimpleExpressionParser
{

    [Production("root : GeneratedGenericSimpleExpressionParser_expressions")]
        
    public double Root(double value) => value;
        
    [Operation((int) GeneratedGenericExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
    [Operation("MINUS", Affix.InFix, Associativity.Left, 10)]
    public double BinaryTermExpression(double left, Token<GeneratedGenericExpressionToken> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case GeneratedGenericExpressionToken.PLUS:
            {
                result = left + right;
                break;
            }
            case GeneratedGenericExpressionToken.MINUS:
            {
                result = left - right;
                break;
            }
        }

        return result;
    }


    [Operation((int) GeneratedGenericExpressionToken.TIMES, Affix.InFix, Associativity.Right, 50)]
    [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
    //[NodeName("multiplication_or_division")]
    public double BinaryFactorExpression(double left, Token<GeneratedGenericExpressionToken> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case GeneratedGenericExpressionToken.TIMES:
            {
                result = left * right;
                break;
            }
            case GeneratedGenericExpressionToken.DIVIDE:
            {
                result = left / right;
                break;
            }
        }

        return result;
    }


    [Prefix((int) GeneratedGenericExpressionToken.MINUS,  Associativity.Right, 100)]
    public double PreFixExpression(Token<GeneratedGenericExpressionToken> operation, double value)
    {
        return -value;
    }

    [Postfix((int) GeneratedGenericExpressionToken.FACTORIAL, Associativity.Right, 110)]
    public double PostFixExpression(double value, Token<GeneratedGenericExpressionToken> operation)
    {
        var factorial = 1;
        for (var i = 1; i <= value; i++) factorial = factorial * i;
        return factorial;
    }

    [Operand]
    [Production("operand : primary_value")]
    // [NodeName("double")]
    public double OperandValue(double value)
    {
        return value;
    }


    [Production("primary_value : DOUBLE")]
    // [NodeName("double")]
    public double OperandDouble(Token<GeneratedGenericExpressionToken> value)
    {
        return value.DoubleValue;
    }
        
    [Production("primary_value : INT")]
    // [NodeName("integer")]
    public double OperandInt(Token<GeneratedGenericExpressionToken> value)
    {
        return value.DoubleValue;
    }

    [Production("primary_value : LPAREN GeneratedGenericSimpleExpressionParser_expressions RPAREN")]
    // [NodeName("group")]
    public double OperandParens(Token<GeneratedGenericExpressionToken> lparen, double value, Token<GeneratedGenericExpressionToken> rparen)
    {
        return value;
    }
}