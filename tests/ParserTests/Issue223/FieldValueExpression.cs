namespace ParserTests.Issue223_EarlyEos
{
    public class FieldValueExpression : Expression
    {
        public Fields? Field { get; }
        public Expression Value { get; }

        public FieldValueExpression(
            Fields? field,
            Expression value)
        {
            Field = field;
            Value = value;
        }

        public override string ToQuery()
        {
            if (Field.HasValue)
                return $"{Field}:{Value.ToQuery()}";
            return $"{Value.ToQuery()}";
        }

        public override string ToString()
        {
            if (Field.HasValue)
                return $"{Field}:{Value}";
            return Value.ToString();
        }
    }
}