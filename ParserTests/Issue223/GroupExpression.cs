namespace ParserTests.Issue223_EarlyEos
{
    public class GroupExpression : Expression
    {
        public Expression Expression { get; }

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