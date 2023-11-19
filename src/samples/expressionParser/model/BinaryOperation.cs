namespace expressionparser.model
{
    public class BinaryOperation : Expression
    {
        private readonly Expression LeftExpresion;
        private readonly ExpressionToken Operator;
        private readonly Expression RightExpression;


        public BinaryOperation(Expression left, ExpressionToken op, Expression right)
        {
            LeftExpresion = left;
            Operator = op;
            RightExpression = right;
        }

        public int? Evaluate(ExpressionContext context)
        {
            var left = LeftExpresion.Evaluate(context);
            var right = RightExpression.Evaluate(context);

            if (left.HasValue && right.HasValue)
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
            return null;
        }
    }
}