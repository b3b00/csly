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
        DIFFERENT,
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

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(OPERATION [{Operator}]");
            dmp.AppendLine($"{Left.Dump("\t"+tab)},");
            dmp.AppendLine(Right.Dump("\t" + tab));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }
    }
}
