using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue184
{
    public class Issue184Parser
    {
        [Operation("PLUS", Affix.InFix, Associativity.Right, 10)]
        [Operation("MINUS", Affix.InFix, Associativity.Left, 10)]
        public double BinaryTermExpression(double left, Token<Issue184Token> operation, double right)
        {
            double result = 0;
            switch (operation.TokenID)
            {
                case Issue184Token.PLUS:
                {
                    result = left + right;
                    break;
                }
                case Issue184Token.MINUS:
                {
                    result = left - right;
                    break;
                }
            }

            return result;
        }


        [Operation("TIMES", Affix.InFix, Associativity.Right, 50)]
        [Operation("DIVIDE", Affix.InFix, Associativity.Left, 50)]
        public double BinaryFactorExpression(double left, Token<Issue184Token> operation, double right)
        {
            double result = 0;
            switch (operation.TokenID)
            {
                case Issue184Token.TIMES:
                {
                    result = left * right;
                    break;
                }
                case Issue184Token.DIVIDE:
                {
                    result = left / right;
                    break;
                }
            }

            return result;
        }

    

        [Operand]
        [Production("double_value : DOUBLE")]
        public double OperandDouble(Token<Issue184Token> value)
        {
            return value.DoubleValue;
        }
        
        [Operand]
        [Production("int_value : INT")]
        public double OperandInt(Token<Issue184Token> value)
        {
            return value.DoubleValue;
        }

        [Operand]
        [Production("group_value : LPAREN Issue184Parser_expressions RPAREN")]
        public double OperandParens(Token<Issue184Token> lparen, double value, Token<Issue184Token> rparen)
        {
            return value;
        }

         
    }
}