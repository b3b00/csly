using System;
using expressionparser;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests
{
    public class ImplicitTokensExpressionParser
    {
        [Production("primary: DOUBLE")]
        [Operand]
        public double Primary(Token<ImplicitTokensTokens> doubleToken)
        {
            return doubleToken.DoubleValue;
        }
            
        [Operand]
        [Production("primary : 'bozzo'[d]")]
        public double Bozzo()
        {
            return 42.0;
        }
    
        [Operand]
        [Production("primary : TEST[d]")]
        public double Test()
        {
            return 0.0;
        }


        [Infix("'+'",Associativity.Left, 10)]
        [Infix("'-'", Associativity.Left, 10)]
        public double BinaryTermExpression(double left, Token<ImplicitTokensTokens> operation, double right)
        {
            switch (operation.Value)
            {
                case "+" : return left + right;
                case "-" : return left - right;
                default : throw new InvalidOperationException($"that is not possible ! {operation.Value} is not a valid operation");
            }
            return 0;
        }
            
        [Operation((int) ImplicitTokensTokens.TIMES, Affix.InFix, Associativity.Left, 50)]
        [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
        public double BinaryFactorExpression(double left, Token<ImplicitTokensTokens> operation, double right)
        {
            double result = 0;
            switch (operation.TokenID)
            {
                case ImplicitTokensTokens.TIMES:
                {
                    result = left * right;
                    break;
                }
                case ImplicitTokensTokens.DIVIDE:
                {
                    result = left / right;
                    break;
                }
            }

            return result;
        }


        [Prefix("'-'", Associativity.Right, 100)]
        public double PreFixExpression(Token<ImplicitTokensTokens> operation, double value)
        {
            return -value;
        }
        
        [Postfix("'!'", Associativity.Left, 110)]
        public double PostFixExpression(double value, Token<ImplicitTokensTokens> operation)
        {
            double factorial = 1;
            for (int i = 0; i < value; i++)
            {
                factorial *= i;
            }
        
            return factorial;
        }
    
      
    
    
    
    
    }
}