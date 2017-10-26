namespace expressionparser.model
{
    public class UnaryOperation : Expression
    {
        private Expression RightExpression;
        private ExpressionToken Operator;
        
        public UnaryOperation(ExpressionToken op, Expression right)
        {
            Operator = op;
            RightExpression = right;
        }

        public int? Evaluate(ExpressionContext context)
        {
            int? right = RightExpression.Evaluate(context);

            if (right.HasValue)
            {

                switch (Operator)
                {
                    case ExpressionToken.PLUS:
                    {
                        return + right.Value;
                    }
                    case ExpressionToken.MINUS:
                    {
                        return - right.Value;
                    }
                    default:
                    {
                        return null;
                    }
                }
            }
            return null;
        }
    }
}