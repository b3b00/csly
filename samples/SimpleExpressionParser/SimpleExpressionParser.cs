using System;
using System.Collections.Generic;
using System.Text;
using expressionparser;
using sly.lexer;
using sly.parser.generator;

namespace simpleExpressionParser
{
    public class SimpleExpressionParser
    {
        
        [Operation((int)ExpressionToken.PLUS, 2, Associativity.Right, 10)]
        [Operation((int)ExpressionToken.MINUS, 2, Associativity.Right, 10)]
        [Operation((int)ExpressionToken.TIMES, 2, Associativity.Right, 50)]
        [Operation((int)ExpressionToken.DIVIDE, 2, Associativity.Right, 50)]
        public int binaryExpression(int left, Token<ExpressionToken> operation, int right)
        {
            int result = 0;
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


        [Operation((int)ExpressionToken.MINUS, 1, Associativity.Right, 100)]
        public  int unaryExpression(Token<ExpressionToken> operation, int value)
        {
            return -value;
        }

        [Operand]
        [Production("operand : INT")]
        public int operand(Token<ExpressionToken> value)
        {
            return value.IntValue;
        }

        
    }
}
