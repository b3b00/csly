using expressionparser;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.issue364;

public class Issue364Parser
{
     [Production("primary: INT")]
        public int Primary(Token<Issue364Token> intToken)
        {
            return intToken.IntValue;
        }

        //[Production("primary: LPAREN [d] expression RPAREN [d]")]
        //public int Group(int groupValue)
        //{
        //    return groupValue;
        //}


        [Production("expression : term PLUS term")]
        public int Expression(int left, Token<Issue364Token> operatorToken, int right)
        {
            var result = 0;


            switch (operatorToken.TokenID)
            {
                case Issue364Token.PLUS:
                {
                    result = left + right;
                    break;
                }
            }

            return result;
        }

        /*[Production("expression : term")]
        public int Expression_Term(int termValue)
        {
            return termValue;
        }

        [Production("term : factor TIMES term")]
        [Production("term : factor DIVIDE term")]
        public int Term(int left, Token<Issue364Token> operatorToken, int right)
        {
            var result = 0;


            switch (operatorToken.TokenID)
            {
                case Issue364Token.TIMES:
                {
                    result = left * right;
                    break;
                }
                case Issue364Token.DIVIDE:
                {
                    result = left / right;
                    break;
                }
            }

            return result;
        }
        */

        [Production("term : factor")]
        public int Term_Factor(int factorValue)
        {
            return factorValue;
        }

        [Production("factor : primary")]
        public int primaryFactor(int primValue)
        {
            return primValue;
        }

        /*[Production("factor : MINUS factor")]
        public int Factor(Token<Issue364Token> discardedMinus, int factorValue)
        {
            return -factorValue;
        }*/
}