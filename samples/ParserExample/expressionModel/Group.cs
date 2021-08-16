namespace ParserExample.expressionModel
{
    public class Group : ParserExample.expressionModel.Expression
    {
        private readonly ParserExample.expressionModel.Expression InnerExpression;

        public Group(ParserExample.expressionModel.Expression expr)
        {
            InnerExpression = expr;
        }

        public double? Evaluate(ParserExample.expressionModel.ExpressionContext context)
        {
            return InnerExpression.Evaluate(context);
        }
    }
}