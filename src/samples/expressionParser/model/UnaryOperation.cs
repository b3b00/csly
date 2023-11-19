namespace expressionparser.model
{
    public class UnaryOperation : Expression
    {
        private readonly ExpressionToken Operator;
        private readonly Expression RightExpression;

        public UnaryOperation(ExpressionToken op, Expression right)
        {
            Operator = op;
            RightExpression = right;
        }

        public int? Evaluate(ExpressionContext context)
        {
            var right = RightExpression.Evaluate(context);

            if (right.HasValue)
                switch (Operator)
                {
                    case ExpressionToken.PLUS:
                    {
                        return +right.Value;
                    }
                    case ExpressionToken.MINUS:
                    {
                        return -right.Value;
                    }
                    default:
                    {
                        return null;
                    }
                }
            return null;
        }
    }
}