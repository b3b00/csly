namespace postProcessedLexerParser.expressionModel
{
    public class Group : Expression
    {
        private readonly Expression InnerExpression;

        public Group(Expression expr)
        {
            InnerExpression = expr;
        }

        public double? Evaluate(ExpressionContext context)
        {
            return InnerExpression.Evaluate(context);
        }
    }
}