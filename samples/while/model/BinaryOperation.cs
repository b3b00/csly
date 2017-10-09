using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.model
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
        CONCAT
    }

    public class BinaryOperation : Expression
    {

        public Expression Left { get; set; }

        public BinaryOperator Operator { get; set; }

        public Expression Right { get; set; }

        public BinaryOperation(Expression left, BinaryOperator oper, Expression right)
        {
            Left = left;
            Operator = oper;
            Right = right;
        }
    }
}
