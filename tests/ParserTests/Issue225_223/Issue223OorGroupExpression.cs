using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class Issue223OorGroupExpression : Issue223OorExpression
    {
        public Issue223OorExpression Issue223OorExpression { get; }
        public override IEnumerable<Issue223OorExpression> Children => new List<Issue223OorExpression> {Issue223OorExpression};

        public Issue223OorGroupExpression(Issue223OorExpression issue223OorExpression)
        {
            Issue223OorExpression = issue223OorExpression;
        }
        
        public override string ToQuery()
        {
            return $"({Issue223OorExpression.ToQuery()})";
        }

        public override string ToString()
        {
            return $"({Issue223OorExpression})";
        }
    }
}