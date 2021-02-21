namespace ParserTests.Issue223_EarlyEos
{
    public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public BinaryOperator Operator { get; }
        public Expression Right { get; }

        public BinaryExpression(Expression left, BinaryOperator @operator, Expression right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }

        public override string ToQuery()
        {
            return $"{Left.ToQuery()} {Operator} {Right.ToQuery()}";
        }

        public override string ToString()
        {
            return $"{Left} {Operator} {Right}";
        }
    }
}