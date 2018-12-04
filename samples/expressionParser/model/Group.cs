namespace expressionparser.model
{
    public class Group : Expression
    {
        private readonly Expression InnerExpression;

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