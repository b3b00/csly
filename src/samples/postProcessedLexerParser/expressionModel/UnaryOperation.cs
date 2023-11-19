namespace postProcessedLexerParser.expressionModel
{
    public class UnaryOperation : Expression
    {
        private readonly FormulaToken Operator;
        private readonly Expression RightExpression;

        public UnaryOperation(FormulaToken op, Expression right)
        {
            Operator = op;
            RightExpression = right;
        }

        public double? Evaluate(ExpressionContext context)
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