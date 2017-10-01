namespace expressionparser.model
{
    public class BinaryOperation : Expression
    {
        private Expression LeftExpresion;
        private Expression RightExpression;
        private ExpressionToken Operator;


        public BinaryOperation(Expression left, ExpressionToken op, Expression right)
        {
            LeftExpresion = left;
            Operator = op;
            RightExpression = right;
        }

        public int? Evaluate(ExpressionContext context)
        {
            int? left = LeftExpresion.Evaluate(context);
            int? right = RightExpression.Evaluate(context);

            if (left.HasValue && right.HasValue)
            {

                switch (Operator)
                {
                    case ExpressionToken.PLUS:
                    {
                        return left.Value + right.Value;
                    }
                    case ExpressionToken.MINUS:
                    {
                        return left.Value - right.Value;
                    }
                    case ExpressionToken.TIMES:
                    {
                        return left.Value * right.Value;
                    }
                    case ExpressionToken.DIVIDE:
                    {
                        return left.Value / right.Value;
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