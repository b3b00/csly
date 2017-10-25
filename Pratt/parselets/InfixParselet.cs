using sly.lexer;

namespace sly.pratt.parselets
{




    /**
     * One of the two parselet interfaces used by the Pratt parser. An
     * InfixParselet is associated with a token that appears in the middle of the
     * expression it parses. Its parse() method will be called after the left-hand
     * side has been parsed, and it in turn is responsible for parsing everything
     * that comes after the token. This is also used for postfix expressions, in
     * which case it simply doesn't consume any more tokens in its parse() call.
     */
    public class InfixParselet<IN, OUT> : Parselet<IN, OUT> where IN : struct
    {

        public BinaryExpressionBuilder<IN, OUT> Builder { get; set; }
        public Associativity Associativity { get; }


        public InfixParselet(IN oper, int precedence, Associativity assoc, BinaryExpressionBuilder<IN, OUT> builder) : base(precedence, oper)
        {
            Associativity = assoc;
            Builder = builder;
        }

        public virtual OUT Parse(Parser<IN,OUT> parser, OUT left, Token<IN> token)
        {
            OUT right = parser.parseExpression();            
            return Builder(token, left, right);
        }
    }
}
