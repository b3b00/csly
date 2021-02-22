namespace ParserTests.Issue223_EarlyEos
{
    public class ValueExpression : Expression
    {
        public string Value { get; }

        public ValueExpression(string value)
        {
            Value = value;
        }

        public override string ToQuery()
        {
            if (Value.Contains(' ')
                || Value.Contains('\'')
                || Value.Contains('"')
                || Value.Contains(':'))
            {
                var escapedValue = Value.Replace(@"\""", @"""").Replace(@"\'", "'").Replace("\"", "\\\"");
                return $"\"{escapedValue}\"";
            }

            return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}