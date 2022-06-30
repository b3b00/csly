using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using sly.lexer;
using sly.parser.generator;


namespace SimpleTemplate.model.expressions
{
    public enum BinaryOperator
    {
        ADD,
        SUB,
        MULTIPLY,
        DIVIDE,
        AND,
        OR,
        GREATER,
        LESSER,
        EQUALS,
        DIFFERENT,
        CONCAT
    }

    public class BinaryOperation : Expression
    {
        public BinaryOperation(Expression left, BinaryOperator oper, Expression right)
        {
            Left = left;
            Operator = oper;
            Right = right;
        }

        public Expression Left { get; set; }

        public BinaryOperator Operator { get; set; }

        public Expression Right { get; set; }

        public LexerPosition Position { get; set; }

        public string GetValue(Dictionary<string, object> context)
        {
            throw new NotImplementedException();
        }

        private T binary<T>(object left, object right, string operationName, Func<T, T, T> operation)
        {
            if (left is T ltb && right is T rt && right.GetType() == left.GetType())
            {
                return operation(ltb,rt);
            }
            else
            {
                throw new TemplateEvaluationException($"cannot {operationName} {left}, {right}");
            }
        }
        
        public object Evaluate(Dictionary<string, object> context)
        {
            object l = Left.Evaluate(context);
            object r = Right.Evaluate(context);
            
            
            

            switch (Operator)
            {
                case BinaryOperator.OR:
                {
                    return binary<bool>(l, r, "OR", (lb, rb) => lb || rb);
                }
                case BinaryOperator.AND:
                {
                    return binary<bool>(l, r, "AND", (lb, rb) => lb && rb);
                }
                case BinaryOperator.ADD:
                {
                    return binary<int>(l, r, "+", (lb, rb) => lb + rb);
                }
                case BinaryOperator.SUB:
                {
                    return binary<int>(l, r, "-", (lb, rb) => lb - rb);
                }
                case BinaryOperator.MULTIPLY:
                {
                    return binary<int>(l, r, "*", (lb, rb) => lb * rb);
                }
                case BinaryOperator.DIVIDE:
                {
                    return binary<int>(l, r, "/", (lb, rb) => lb / rb);
                }
                case BinaryOperator.EQUALS:
                {
                    return binary<IComparable>(l, r, "==", (lb, rb) => lb.CompareTo(rb) == 0);
                }
                case BinaryOperator.DIFFERENT:
                {
                    return binary<IComparable>(l, r, "==", (lb, rb) => lb.CompareTo(rb) != 0);
                }
                case BinaryOperator.LESSER:
                {
                    return binary<IComparable>(l, r, "<", (lb, rb) => lb.CompareTo(rb) < 0);
                }
                case BinaryOperator.GREATER:
                {
                    return binary<IComparable>(l, r, ">", (lb, rb) => lb.CompareTo(rb) > 0);
                }
                case BinaryOperator.CONCAT:
                {
                    return binary<string>(l.ToString(), r.ToString(), ".", (lb, rb) => lb + rb);
                }
                default:
                {
                    return $"dont know what to do with {Operator}";
                }
            }
        }
    }
}