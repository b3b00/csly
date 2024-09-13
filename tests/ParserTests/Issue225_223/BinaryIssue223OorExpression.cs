using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class BinaryIssue223OorExpression : Issue223OorExpression
    {
        public Issue223OorExpression Left { get; }
        public Issue223OorBinaryOperator Operator { get; }
        public Issue223OorExpression Right { get; }
        public override IEnumerable<Issue223OorExpression> Children => new List<Issue223OorExpression> {Left, Right};

        public BinaryIssue223OorExpression(Issue223OorExpression left, Issue223OorBinaryOperator @operator, Issue223OorExpression right)
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