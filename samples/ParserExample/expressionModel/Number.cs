namespace ParserExample.expressionModel
{
    public sealed class Number : ParserExample.expressionModel.Expression
    {
        private readonly double Value;

        public Number(double value)
        {
            Value = value;
        }

        public double? Evaluate(ParserExample.expressionModel.ExpressionContext context)
        {
            return Value;
        }
    }
}