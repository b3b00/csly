using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class Issue223OorFieldValueExpression : Issue223OorExpression
    {
        public Issue223OorFields? Field { get; }
        public Issue223OorExpression Value { get; }
        public override IEnumerable<Issue223OorExpression> Children => new List<Issue223OorExpression> {Value};

        public Issue223OorFieldValueExpression(
            Issue223OorFields? field,
            Issue223OorExpression value)
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