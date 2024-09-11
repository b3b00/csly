using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class Issue223OorValueExpression : Issue223OorExpression
    {
        public string Value { get; }
        public override IEnumerable<Issue223OorExpression> Children => new Issue223OorExpression[0];

        public Issue223OorValueExpression(string value)
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