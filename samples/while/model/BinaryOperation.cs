using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

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

        public Scope CompilerScope { get; set; }

        public TokenPosition Position { get; set; }

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
            dmp.AppendLine($"{Left.Dump("\t" + tab)},");
            dmp.AppendLine(Right.Dump("\t" + tab));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            var operators = new Dictionary<BinaryOperator, string>()
            {
                {BinaryOperator.ADD," + " },
                {BinaryOperator.SUB," - "},
                { BinaryOperator.MULTIPLY," * "},
                { BinaryOperator.DIVIDE," / "},
                { BinaryOperator.EQUALS," == " },
                { BinaryOperator.DIFFERENT," != "},
                { BinaryOperator.OR," || "},
                { BinaryOperator.AND," && "},
                { BinaryOperator.LESSER," < "},
                { BinaryOperator.GREATER," + "},
                { BinaryOperator.CONCAT," + "}
            };
            StringBuilder code = new StringBuilder();
            code.Append(Left.Transpile(context));
            code.Append(operators[Operator]);
            code.Append(Right.Transpile(context));
            return code.ToString();
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            emiter = Left.EmitByteCode(context,emiter);
            emiter = Right.EmitByteCode(context,emiter);
            switch (Operator)
            {
                case BinaryOperator.ADD: {
                        emiter = emiter.Add();
                        break;
                    }
                case BinaryOperator.SUB: {
                        emiter = emiter.Subtract();
                        break;
                    }
                case BinaryOperator.MULTIPLY: {
                        emiter = emiter.Multiply();
                        break;
                    }
                case BinaryOperator.DIVIDE: {
                        emiter = emiter.Divide();
                        break;
                    }
                case BinaryOperator.EQUALS: {
                        emiter = emiter.CompareEqual();
                        break;
                    }
                case BinaryOperator.DIFFERENT: {
                        emiter = emiter.CompareEqual();
                        emiter = emiter.Not();
                            break;
                    }
                case BinaryOperator.OR: {
                        emiter = emiter.Or();
                        break;
                    }
                case BinaryOperator.AND: {
                        emiter = emiter.And();
                        break;
                    }
                case BinaryOperator.LESSER: {
                        emiter = emiter.CompareLessThan();
                        break;
                    }
                case BinaryOperator.GREATER: {
                        emiter = emiter.CompareGreaterThan();
                        break;
                    }
            }
            return emiter;
        }
    }
}
