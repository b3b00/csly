namespace ParserExample.expressionModel
{
    public class UnaryOperation : ParserExample.expressionModel.Expression
    {
        private readonly FormulaToken Operator;
        private readonly ParserExample.expressionModel.Expression RightExpression;

        public UnaryOperation(FormulaToken op, ParserExample.expressionModel.Expression right)
        {
            Operator = op;
            RightExpression = right;
        }

        public double? Evaluate(ParserExample.expressionModel.ExpressionContext context)
        {
            var right = RightExpression.Evaluate(context);

            if (right.HasValue)
                switch (Operator)
                {
                    case FormulaToken.PLUS:
                    {
                        return +right.Value;
                    }
                    case FormulaToken.MINUS:
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