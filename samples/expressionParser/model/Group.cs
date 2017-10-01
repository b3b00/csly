namespace expressionparser.model
{
    public class Group : Expression
    {
        private Expression InnerExpression;

        public Group(Expression expr)
        {
            InnerExpression = expr;
        }

        public int? Evaluate(ExpressionContext context)
        {
            return InnerExpression.Evaluate(context);
        }
    }
}