using sly.lexer;

namespace sly.pratt.parselets
{



    /**
     * One of the two interfaces used by the Pratt parser. A PrefixParselet is
     * associated with a token that appears at the beginning of an expression. Its
     * parse() method will be called with the consumed leading token, and the
     * parselet is responsible for parsing anything that comes after that token.
     * This interface is also used for single-token expressions like variables, in
     * which case parse() simply doesn't consume any more tokens.
     * @author rnystrom
     *
     */
    public class PrefixParselet<IN,OUT> : Parselet<IN,OUT> where IN : struct
    {

        public UnaryExpressionBuilder<IN, OUT> Builder { get; set; } 


        public PrefixParselet(IN oper, int precedence, UnaryExpressionBuilder<IN, OUT> builder) : base(precedence, oper)
        {
            Builder = builder;
        }

        public OUT Parse(Parser<IN,OUT> parser, Token<IN> token)
        {
            OUT right = parser.parseExpression(Precedence);
            return Builder(token, right);
        }
    }

}