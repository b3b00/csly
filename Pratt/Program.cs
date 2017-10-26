using System.Collections.Generic;
using System.Linq;
using expressionparser;
using sly.lexer;
using sly.pratt;
using sly.pratt.parselets;

namespace Pratt
{
    class Program
    {

        static int unary(Token<ExpressionToken> tok, int val)
        {
            if (tok.TokenID == ExpressionToken.MINUS)
            {
                return -val;
            }
            else if (tok.TokenID == ExpressionToken.INT)
            {
                return tok.IntValue;
            }
            throw new System.Exception("don't known");
        }


        static int binary(Token<ExpressionToken> tok, int left, int right)
        {
            switch(tok.TokenID)
            {
                case ExpressionToken.PLUS:
                    {
                        return left + right;
                    }
                case ExpressionToken.MINUS:
                    {
                        return left - right;
                    }
                case ExpressionToken.TIMES:
                    {
                        return left * right;
                    }
                case ExpressionToken.DIVIDE:
                    {
                        return left / right;                        
                    }
            }
            return 0;
        }

        static void Main(string[] args)
        {
            ILexer<ExpressionToken> lexer = LexerBuilder.BuildLexer<ExpressionToken>();

            List<Token<ExpressionToken>> tokens = lexer.Tokenize("1 + 2 + 3").ToList();

            Parser<ExpressionToken, int> parser = new Parser<ExpressionToken, int>(tokens, 0);

            UnaryExpressionBuilder<ExpressionToken,int> ub = (Token<ExpressionToken> t, int i) => { return unary(t, i); };
            BinaryExpressionBuilder<ExpressionToken, int> bb = (Token<ExpressionToken> t, int i, int j) => { return binary(t, i,j); };

            parser.prefix(ExpressionToken.MINUS, 100, ub);
            parser.prefix(ExpressionToken.INT, 150, ub);
            parser.infix(ExpressionToken.MINUS, 50, bb);
            parser.infix(ExpressionToken.PLUS, 50, bb);

            parser.parseExpression();


        }
    }
}
