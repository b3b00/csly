using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class BoostExpression : Expression
    {
        public Expression Expression { get; }
        public int Boost { get; }

        public override IEnumerable<Expression> Children => new List<Expression> {Expression};

        public BoostExpression(Expression expression, int boost)
        {
            Expression = expression;
            Boost = boost;
        }
        
        public override string ToQuery()
        {
            return $"{Expression.ToQuery()}^{Boost}";
        }

        public override string ToString()
        {
            return $"{Expression}^{Boost}";
        }
    }
}