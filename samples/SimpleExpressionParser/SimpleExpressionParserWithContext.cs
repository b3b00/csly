using System.Collections.Generic;
using expressionparser;
using sly.lexer;
using sly.parser.generator;

namespace simpleExpressionParser
{
    public class SimpleExpressionParserWithContext
    
    {
        [Operation((int) ExpressionToken.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) ExpressionToken.MINUS, Affix.InFix, Associativity.Left, 10)]
        public int BinaryTermExpression(int left, Token<ExpressionToken> operation, int right,Dictionary<string,int> context)
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
        public int BinaryFactorExpression(int left, Token<ExpressionToken> operation, int right,Dictionary<string,int> context)
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
        public int PreFixExpression(Token<ExpressionToken> operation, int value,Dictionary<string,int> context)
        {
            return -value;
        }

        [Operation((int) ExpressionToken.FACTORIAL, Affix.PostFix, Associativity.Right, 100)]
        public int PostFixExpression(int value, Token<ExpressionToken> operation,Dictionary<string,int> context)
        {
            var factorial = 1;
            for (var i = 1; i <= value; i++) factorial = factorial * i;
            return factorial;
        }

        [Operand]
        [Production("operand : primary_value")]
        public int OperandValue(int value,Dictionary<string,int> context)
        {
            return value;
        }

        
        [Production("primary_value : IDENTIFIER")]
        public int OperandVariable(Token<ExpressionToken> identifier,Dictionary<string,int> context)
        {
            if (context.ContainsKey(identifier.Value))
            {
                return context[identifier.Value];
            }
            else
            {
                return 0;
            }
        }

        [Production("primary_value : INT")]
        public int OperandInt(Token<ExpressionToken> value,Dictionary<string,int> context)
        {
            return value.IntValue;
        }

        [Production("primary_value : LPAREN SimpleExpressionParserWithContext_expressions RPAREN")]
        public int OperandParens(Token<ExpressionToken> lparen, int value, Token<ExpressionToken> rparen,Dictionary<string,int> context)
        {
            return value;
        }
    }
}