using com.stuffwithstuff.bantam;

namespace com.stuffwithstuff.bantam.expressions
{

    /**
     * A prefix unary arithmetic expression like "!a" or "-b".
     */
    public class PrefixExpression : Expression
    {
        public PrefixExpression(TokenType operator, Expression right)
        {
            mOperator = operator;
            mRight = right;
        }

        public void print(StringBuilder builder)
        {
            builder.append("(").append(mOperator.punctuator());
            mRight.print(builder);
            builder.append(")");
        }

        private TokenType mOperator;
        private Expression mRight;
    }

}