using expressionparser;
using sly.lexer;
using sly.parser.generator;

namespace simpleExpressionParser
{
    public class SimpleExpressionParser
    {
        [Operation((int) ExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) ExpressionToken.MINUS, Affix.InFix, Associativity.Left, 10)]
        public int BinaryTermExpression(int left, Token<ExpressionToken> operation, int right)
        {
            var result = 0;
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
        [Operation((int) ExpressionToken.DIVIDE, Affix.InFix, Associativity.Left, 50)]
        public int BinaryFactorExpression(int left, Token<ExpressionToken> operation, int right)
        {
            var result = 0;
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


        [Operation((int) ExpressionToken.MINUS, Affix.PreFix, Associativity.Right, 100)]
        public int PreFixExpression(Token<ExpressionToken> operation, int value)
        {
            return -value;
        }

        [Operation((int) ExpressionToken.FACTORIAL, Affix.PostFix, Associativity.Right, 100)]
        public int PostFixExpression(int value, Token<ExpressionToken> operation)
        {
            var factorial = 1;
            for (var i = 1; i <= value; i++) factorial = factorial * i;
            return factorial;
        }

        [Operand]
        [Production("operand : primary_value")]
        public int OperandValue(int value)
        {
            return value;
        }


        [Production("primary_value : INT")]
        public int OperandInt(Token<ExpressionToken> value)
        {
            return value.IntValue;
        }

        [Production("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN")]
        public int OperandParens(Token<ExpressionToken> lparen, int value, Token<ExpressionToken> rparen)
        {
            return value;
        }
    }
}