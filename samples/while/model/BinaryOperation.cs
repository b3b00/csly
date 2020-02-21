using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using csly.whileLang.compiler;
using sly.lexer;
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
        public BinaryOperation(Expression left, BinaryOperator oper, Expression right)
        {
            Left = left;
            Operator = oper;
            Right = right;
        }

        public Expression Left { get; set; }

        public BinaryOperator Operator { get; set; }

        public Expression Right { get; set; }

        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }
        public WhileType Whiletype { get; set; }

        [ExcludeFromCodeCoverage]
        public string Dump(string tab)
        {
            var dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(OPERATION [{Operator}]");
            dmp.AppendLine($"{Left.Dump("\t" + tab)},");
            dmp.AppendLine(Right.Dump("\t" + tab));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            var operators = new Dictionary<BinaryOperator, string>
            {
                {BinaryOperator.ADD, " + "},
                {BinaryOperator.SUB, " - "},
                {BinaryOperator.MULTIPLY, " * "},
                {BinaryOperator.DIVIDE, " / "},
                {BinaryOperator.EQUALS, " == "},
                {BinaryOperator.DIFFERENT, " != "},
                {BinaryOperator.OR, " || "},
                {BinaryOperator.AND, " && "},
                {BinaryOperator.LESSER, " < "},
                {BinaryOperator.GREATER, " + "},
                {BinaryOperator.CONCAT, " + "}
            };
            var code = new StringBuilder();
            code.Append(Left.Transpile(context));
            code.Append(operators[Operator]);
            code.Append(Right.Transpile(context));
            return code.ToString();
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            if (Operator == BinaryOperator.CONCAT) return EmitConcat(context, emiter);
            emiter = Left.EmitByteCode(context, emiter);
            emiter = Right.EmitByteCode(context, emiter);
            switch (Operator)
            {
                case BinaryOperator.ADD:
                {
                    emiter = emiter.Add();
                    break;
                }
                case BinaryOperator.SUB:
                {
                    emiter = emiter.Subtract();
                    break;
                }
                case BinaryOperator.MULTIPLY:
                {
                    emiter = emiter.Multiply();
                    break;
                }
                case BinaryOperator.DIVIDE:
                {
                    emiter = emiter.Divide();
                    break;
                }
                case BinaryOperator.EQUALS:
                {
                    emiter = emiter.CompareEqual();
                    break;
                }
                case BinaryOperator.DIFFERENT:
                {
                    emiter = emiter.CompareEqual();
                    emiter = emiter.Not();
                    break;
                }
                case BinaryOperator.OR:
                {
                    emiter = emiter.Or();
                    break;
                }
                case BinaryOperator.AND:
                {
                    emiter = emiter.And();
                    break;
                }
                case BinaryOperator.LESSER:
                {
                    emiter = emiter.CompareLessThan();
                    break;
                }
                case BinaryOperator.GREATER:
                {
                    emiter = emiter.CompareGreaterThan();
                    break;
                }
            }

            return emiter;
        }

        private Emit<Func<int>> EmitConcat(CompilerContext context, Emit<Func<int>> emiter)
        {
            var typer = new ExpressionTyper();
            Left.EmitByteCode(context, emiter);
            if (Left.Whiletype != WhileType.STRING)
            {
                var t = TypeConverter.WhileToType(Left.Whiletype);
                emiter.Box(t);
            }

            Right.EmitByteCode(context, emiter);
            if (Right.Whiletype != WhileType.STRING)
            {
                var t = TypeConverter.WhileToType(Right.Whiletype);
                emiter.Box(t);
            }

            var mi = typeof(string).GetMethod("Concat", new[] {typeof(object), typeof(object)});
            emiter.Call(mi);
            return emiter;
        }
    }
}