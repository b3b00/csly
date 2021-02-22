namespace ParserTests.Issue223_EarlyEos
{
    public class UnaryExpression : Expression
    {
        public UnaryOperator Operator { get; }
        public Expression Expression { get; }

        public UnaryExpression(UnaryOperator @operator, Expression expression)
        {
            Operator = @operator;
            Expression = expression;
        }

        public override string ToQuery()
        {
            var unaryOperatorString = "";
            switch (Operator)
            {
                case UnaryOperator.NOT:
                    unaryOperatorString = "Not ";
                    // unaryOperatorString = "-";
                    break;
            }

            return $"{unaryOperatorString}{Expression.ToQuery()}";
        }

        public override string ToString()
        {
            if (Operator == UnaryOperator.NONE)
                return Expression.ToString();
            return $"{Operator} {Expression}";
        }
    }
}