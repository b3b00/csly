using System;

namespace postProcessedLexerParser.expressionModel
{
    public class BinaryOperation : Expression
    {
        private readonly Expression LeftExpresion;
        private readonly FormulaToken Operator;
        private readonly Expression RightExpression;


        public BinaryOperation(Expression left, FormulaToken op, Expression right)
        {
            LeftExpresion = left;
            Operator = op;
            RightExpression = right;
        }

        public double? Evaluate(ExpressionContext context)
        {
            var left = LeftExpresion.Evaluate(context);
            var right = RightExpression.Evaluate(context);

            if (left.HasValue && right.HasValue)
                switch (Operator)
                {
                    case FormulaToken.PLUS:
                    {
                        return left.Value + right.Value;
                    }
                    case FormulaToken.MINUS:
                    {
                        return left.Value - right.Value;
                    }
                    case FormulaToken.TIMES:
                    {
                        return left.Value * right.Value;
                    }
                    case FormulaToken.DIVIDE:
                    {
                        return left.Value / right.Value;
                    }
                    case FormulaToken.EXP:
                    {
                        return Math.Pow(left.Value,right.Value);
                    }
                    default:
                    {
                        return null;
                    }
                }
            return null;
        }
    }
}