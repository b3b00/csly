using expressionparser;
using sly.lexer;
using sly.parser.generator;

namespace ParserExample.sourceGenerator
{
    public class TestParser
    {
        [Production("primary: INT")]
        public int Primary(Token<TestLexer> intToken)
        {
            return intToken.IntValue;
        }

        [Production("primary: LPAREN [d] expression RPAREN [d]")]
        public int Group(int groupValue)
        {
            return groupValue;
        }


        [Production("expression : term PLUS expression")]
        [Production("expression : term MINUS expression")]
        public int Expression(int left, Token<TestLexer> operatorToken, int right)
        {
            var result = 0;


            switch (operatorToken.TokenID)
            {
                case TestLexer.PLUS:
                {
                    result = left + right;
                    break;
                }
                case TestLexer.MINUS:
                {
                    result = left - right;
                    break;
                }
            }

            return result;
        }

        [Production("expression : term")]
        public int Expression_Term(int termValue)
        {
            return termValue;
        }

        [Production("term : factor TIMES term")]
        [Production("term : factor DIVIDE term")]
        public int Term(int left, Token<TestLexer> operatorToken, int right)
        {
            var result = 0;


            switch (operatorToken.TokenID)
            {
                case TestLexer.TIMES:
                {
                    result = left * right;
                    break;
                }
                case TestLexer.DIVIDE:
                {
                    result = left / right;
                    break;
                }
            }

            return result;
        }

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

        [Production("factor : MINUS factor")]
        public int Factor(Token<TestLexer> discardedMinus, int factorValue)
        {
            return -factorValue;
        }
    }
}