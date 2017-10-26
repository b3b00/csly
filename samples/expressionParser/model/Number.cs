namespace expressionparser.model
{
    public sealed class Number : Expression
    {
        private int Value;

        public Number(int value)
        {
            Value = value;
        }
        
        public int? Evaluate(ExpressionContext context)
        {
            return Value;
        }
    }
}