using System.Collections.Generic;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class Issue223OorUnaryExpression : Issue223OorExpression
    {
        public Issue223OorUnaryOperator Operator { get; }
        public Issue223OorExpression Issue223OorExpression { get; }
        public override IEnumerable<Issue223OorExpression> Children => new List<Issue223OorExpression> {Issue223OorExpression};

        public Issue223OorUnaryExpression(Issue223OorUnaryOperator @operator, Issue223OorExpression issue223OorExpression)
        {
            Operator = @operator;
            Issue223OorExpression = issue223OorExpression;
        }

        public override string ToQuery()
        {
            var unaryOperatorString = "";
            switch (Operator)
            {
                case Issue223OorUnaryOperator.NOT:
                    unaryOperatorString = "Not ";
                    // unaryOperatorString = "-";
                    break;
            }

            return $"{unaryOperatorString}{Issue223OorExpression.ToQuery()}";
        }

        public override string ToString()
        {
            if (Operator == Issue223OorUnaryOperator.NONE)
                return Issue223OorExpression.ToString();
            return $"{Operator} {Issue223OorExpression}";
        }
    }
}