namespace postProcessedLexerParser.expressionModel
{
    public sealed class Number : Expression
    {
        private readonly double Value;

        public Number(double value)
        {
            Value = value;
        }

        public double? Evaluate(ExpressionContext context)
        {
            return Value;
        }
    }
}