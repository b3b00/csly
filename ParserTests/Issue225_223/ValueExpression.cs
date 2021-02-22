using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class ValueExpression : Expression
    {
        public string Value { get; }
        public override IEnumerable<Expression> Children => new Expression[0];

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