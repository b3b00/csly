using expressionparser;
using sly.lexer;
using sly.parser.generator;

namespace simpleExpressionParser
{
    [ParserRoot("root")]
    //[BroadenTokenWindow]
    public class SimpleExpressionParser
    {

        [Production("root : SimpleExpressionParser_expressions")]
        
        public double Root(double value) => value;
        
        [Operation((int) ExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation("MINUS", Affix.InFix, Associativity.Left, 10)]
        public double BinaryTermExpression(double left, Token<ExpressionToken> operation, double right)
        {
            double result = 0;
            switch (operation.TokenID)
            {
                case ExpressionToken.PLUS:
                {
                    result = left + right;
                    break;
                }
                case ExpressionToken.MINUS:
                {
                    result = left - right;
                    break;
                }
            }

            return result;
        }


        [Operation((int) ExpressionToken.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
        [NodeName("multiplication_or_division")]
        public double BinaryFactorExpression(double left, Token<ExpressionToken> operation, double right)
        {
            double result = 0;
            switch (operation.TokenID)
            {
                case ExpressionToken.TIMES:
                {
                    result = left * right;
                    break;
                }
                case ExpressionToken.DIVIDE:
                {
                    result = left / right;
                    break;
                }
            }

            return result;
        }


        [Prefix((int) ExpressionToken.MINUS,  Associativity.Right, 100)]
        public double PreFixExpression(Token<ExpressionToken> operation, double value)
        {
            return -value;
        }

        [Postfix((int) ExpressionToken.FACTORIAL, Associativity.Right, 100)]
        public double PostFixExpression(double value, Token<ExpressionToken> operation)
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
        public double OperandDouble(Token<ExpressionToken> value)
        {
            return value.DoubleValue;
        }
        
        [Production("primary_value : INT")]
        [NodeName("integer")]
        public double OperandInt(Token<ExpressionToken> value)
        {
            return value.DoubleValue;
        }

        [Production("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN")]
        [NodeName("group")]
        public double OperandParens(Token<ExpressionToken> lparen, double value, Token<ExpressionToken> rparen)
        {
            return value;
        }
    }
    
    
    //[BroadenTokenWindow]
}