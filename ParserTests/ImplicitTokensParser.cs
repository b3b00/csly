using sly.lexer;
using sly.parser.generator;

namespace ParserTests
{
    public class ImplicitTokensParser
    {
        [Production("primary: DOUBLE")]
        public double Primary(Token<ImplicitTokensTokens> doubleToken)
        {
            return doubleToken.DoubleValue;
        }
        
        [Production("primary : 'bozzo'[d]")]
        public double Bozzo()
        {
            return 42.0;
        }

        [Production("primary : TEST[d]")]
        public double Test()
        {
            return 0.0;
        } 


        [Production("expression : primary ['+' | '-'] expression")]
        
        
        public double Expression(double left, Token<ImplicitTokensTokens> operatorToken, double right)
        {
            double result = 0.0;


            switch (operatorToken.StringWithoutQuotes)
            {
                case "+":
                {
                    result = left + right;
                    break;
                }
                case "-":
                {
                    result = left - right;
                    break;
                }
            }

            return result;
        }


        [Production("expression : primary ")]
        public double Simple(double value)
        {
            return value;
        }




    }
}