using System;
using sly.lexer;

namespace BravoLights.Common.Ast
{
    enum NumericOperator
    {
        Plus,
        Minus,
        Times,
        Divide,
        BinaryAnd,
        BinaryOr
    }

    /// <summary>
    /// A binary expression such as 'X + Y' or 'X / Y', which produces a number from two other numbers and an operator.
    /// </summary>
    abstract class BinaryNumericExpression : BinaryExpression<double>
    {
        protected BinaryNumericExpression(IAstNode lhs, IAstNode rhs) : base(lhs, rhs)
        {
        }

        protected abstract double ComputeNumericValue(double lhs, double rhs);

        protected override object ComputeValue(object lhsValue, object rhsValue)
        {
            if (lhsValue is Exception)
            {
                return lhsValue;
            }
            if (rhsValue is Exception)
            {
                return rhsValue;
            }
            var lhs = Convert.ToDouble(lhsValue);
            var rhs = Convert.ToDouble(rhsValue);
            return ComputeNumericValue(lhs, rhs);
        }

        public static BinaryNumericExpression Create(IAstNode lhs, Token<ExpressionToken> op, IAstNode rhs)
        {
            return op.Value switch
            {
                "+" => new PlusExpression(lhs, rhs),
                "-" => new MinusExpression(lhs, rhs),
                "*" => new TimesExpression(lhs, rhs),
                "/" => new DivideExpression(lhs, rhs),
                "&" => new BitwiseAndExpression(lhs, rhs),
                "|" => new BitwiseOrExpression(lhs, rhs),
                _ => throw new Exception($"Unexpected operator: {op.Value}"),
            };
        }
    }

    class PlusExpression : BinaryNumericExpression
    {
        public PlusExpression(IAstNode lhs, IAstNode rhs) : base(lhs, rhs)
        {
        }

        protected override string OperatorText => "+";

        protected override double ComputeNumericValue(double lhs, double rhs)
        {
            return lhs + rhs;
        }
    }

    class MinusExpression : BinaryNumericExpression
    {
        public MinusExpression(IAstNode lhs, IAstNode rhs) : base(lhs, rhs)
        {
        }

        protected override string OperatorText => "-";

        protected override double ComputeNumericValue(double lhs, double rhs)
        {
            return lhs - rhs;
        }
    }

    class TimesExpression : BinaryNumericExpression
    {
        public TimesExpression(IAstNode lhs, IAstNode rhs) : base(lhs, rhs)
        {
        }

        protected override string OperatorText => "*";

        protected override double ComputeNumericValue(double lhs, double rhs)
        {
            return lhs * rhs;
        }
    }

    class DivideExpression : BinaryNumericExpression
    {
        public DivideExpression(IAstNode lhs, IAstNode rhs) : base(lhs, rhs)
        {
        }

        protected override string OperatorText => "/";

        protected override double ComputeNumericValue(double lhs, double rhs)
        {
            return lhs / rhs;
        }
    }

    class BitwiseOrExpression : BinaryNumericExpression
    {
        public BitwiseOrExpression(IAstNode lhs, IAstNode rhs) : base(lhs, rhs)
        {
        }

        protected override string OperatorText => "|";

        protected override double ComputeNumericValue(double lhs, double rhs)
        {
            return Convert.ToDouble(Convert.ToInt32(lhs) | Convert.ToInt32(rhs));
        }
    }

    class BitwiseAndExpression : BinaryNumericExpression
    {
        public BitwiseAndExpression(IAstNode lhs, IAstNode rhs) : base(lhs, rhs)
        {
        }

        protected override string OperatorText => "&";

        protected override double ComputeNumericValue(double lhs, double rhs)
        {
            return Convert.ToDouble(Convert.ToInt32(lhs) & Convert.ToInt32(rhs));
        }
    }
}
