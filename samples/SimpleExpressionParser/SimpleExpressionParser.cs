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
        
      
        [Operation((int)ExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int)ExpressionToken.MINUS, Affix.InFix, Associativity.Left, 10)]
        public int binaryTermExpression(int left, Token<ExpressionToken> operation, int right)
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
            }
            return result;
        }

        
        [Operation((int)ExpressionToken.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int)ExpressionToken.DIVIDE, Affix.InFix, Associativity.Left, 50)]
        public int binaryFactorExpression(int left, Token<ExpressionToken> operation, int right)
        {
            int result = 0;
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


        [Operation((int)ExpressionToken.MINUS, Affix.PreFix, Associativity.Right, 100)]
        public  int unaryExpression(Token<ExpressionToken> operation, int value)
        {
            return -value;
        }

        [Operand]
        [Production("operand : primary_value")]        
        public int operand(int value)
        {
            return value;
        }


        [Production("primary_value : INT")]
        public int operand1(Token<ExpressionToken> value)
        {
            return value.IntValue;
        }

        [Production("primary_value : LPAREN SimpleExpressionParser_expressions RPAREN")]
        public int operand2(Token<ExpressionToken> lparen, int value, Token<ExpressionToken> rparen)
        {
            return value;
        }



    }
}
