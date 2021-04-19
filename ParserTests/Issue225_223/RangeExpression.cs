using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class RangeExpression : Expression
    {
        public ValueExpression From { get; }
        public ValueExpression To { get; }
        
        public override bool IsRange => true;
        public override IEnumerable<Expression> Children => new List<Expression> {From, To};

        public RangeExpression(ValueExpression from, ValueExpression to)
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