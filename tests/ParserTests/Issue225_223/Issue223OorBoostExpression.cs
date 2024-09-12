using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class Issue223OorBoostExpression : Issue223OorExpression
    {
        public Issue223OorExpression Issue223OorExpression { get; }
        public int Boost { get; }

        public override IEnumerable<Issue223OorExpression> Children => new List<Issue223OorExpression> {Issue223OorExpression};

        public Issue223OorBoostExpression(Issue223OorExpression issue223OorExpression, int boost)
        {
            Issue223OorExpression = issue223OorExpression;
            Boost = boost;
        }
        
        public override string ToQuery()
        {
            return $"{Issue223OorExpression.ToQuery()}^{Boost}";
        }

        public override string ToString()
        {
            return $"{Issue223OorExpression}^{Boost}";
        }
    }
}