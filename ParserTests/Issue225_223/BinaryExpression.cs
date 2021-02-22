using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public BinaryOperator Operator { get; }
        public Expression Right { get; }
        public override IEnumerable<Expression> Children => new List<Expression> {Left, Right};

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