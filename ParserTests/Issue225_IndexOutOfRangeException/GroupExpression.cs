using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class GroupExpression : Expression
    {
        public Expression Expression { get; }
        public override IEnumerable<Expression> Children => new List<Expression> {Expression};

        public GroupExpression(Expression expression)
        {
            Expression = expression;
        }
        
        public override string ToQuery()
        {
            return $"({Expression.ToQuery()})";
        }

        public override string ToString()
        {
            return $"({Expression})";
        }
    }
}