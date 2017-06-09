using sly.lexer;
using sly.parser.generator;



namespace expressionparser
{
   


    public class ExpressionParser
    {



        [LexerConfigurationAttribute]
        public ILexer<ExpressionToken> BuildExpressionLexer(ILexer<ExpressionToken> lexer)
        {            
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.DOUBLE, "[0-9]+\\.[0-9]+"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.INT, "[0-9]+"));            
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.PLUS, "\\+"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.MINUS, "\\-"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.TIMES, "\\*"));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.DIVIDE, "\\/"));

            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.LPAREN, "\\("));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.RPAREN, "\\)"));

            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<ExpressionToken>(ExpressionToken.EOL, "[\\n\\r]+", true, true));
            return lexer;
        }




        [Production("primary: INT")]
        public object Primary(Token<ExpressionToken> intToken)
        {
            return intToken.IntValue;
        }

        [Production("primary: LPAREN expression RPAREN")]
        public object Group(object discaredLParen, int groupValue ,object discardedRParen)
        {
            return groupValue;
        }



        [Production("expression : term PLUS expression")]
        [Production("expression : term MINUS expression")]

        public object Expression(int left, Token<ExpressionToken> operatorToken, int  right)
        {
            object result = 0;
            

            switch (operatorToken.TokenID)
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
                default:
                    {
                        break;
                    }
            }
            return result;
        }

        [Production("expression : term")]
        public object Expression_Term(int termValue)
        {           
            return termValue;
        }

        [Production("term : factor TIMES term")]
        [Production("term : factor DIVIDE term")]
        public object Term(int left, Token<ExpressionToken> operatorToken, int right)
        {
            int result = 0;

          
          
            switch (operatorToken.TokenID)
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
                default:
                    {
                        break;
                    }
            }
            return result;
        }

        [Production("term : factor")]
        public object Term_Factor(int factorValue)
        {
            return factorValue;
        }

        [Production("factor : primary")]
        public object primaryFactor(int primValue)
        {
            return primValue;
        }
        [Production("factor : MINUS factor")]
        public object Factor(Token<ExpressionToken> discardedMinus, int factorValue)
        {            
            return -factorValue;
        }




    }
}

