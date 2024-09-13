using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class Issue223OorRangeExpression : Issue223OorExpression
    {
        public Issue223OorValueExpression From { get; }
        public Issue223OorValueExpression To { get; }
        
        public override bool IsRange => true;
        public override IEnumerable<Issue223OorExpression> Children => new List<Issue223OorExpression> {From, To};

        public Issue223OorRangeExpression(Issue223OorValueExpression from, Issue223OorValueExpression to)
        {
            From = @from;
            To = to;
        }
        
        public override string ToQuery()
        {
            return $"[{From.ToQuery()} TO {To.ToQuery()}]";
        }

        public override string ToString()
        {
            return $"[{From} TO {To}]";
        }
    }
}