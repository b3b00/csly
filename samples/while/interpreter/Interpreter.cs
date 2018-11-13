using System;
using System.Collections.Generic;
using System.Text;
using csly.whileLang.model;

namespace csly.whileLang.interpreter
{
    public class InterpreterException : Exception
    {
        public InterpreterException(string message) : base(message)
        {
        }
    }

    public enum WhileType
    {
        BOOL,
        INT,
        STRING,
        ANY
    }

    public class TypedValue
    {
        public TypedValue(WhileType valType, object val)
        {
            ValueType = valType;
            Value = val;
        }

        public TypedValue(string val)
        {
            ValueType = WhileType.STRING;
            Value = val;
        }

        public TypedValue(int val)
        {
            ValueType = WhileType.INT;
            Value = val;
        }


        public TypedValue(bool val)
        {
            ValueType = WhileType.BOOL;
            Value = val;
        }

        public WhileType ValueType { get; }
        public int IntValue => (int) Value;

        public bool BoolValue => (bool) Value;

        public string StringValue => Value.ToString();

        public object Value { get; }

        public override string ToString()
        {
            return $"{StringValue}({ValueType})";
        }
    }

    public class InterpreterContext
    {
        public Dictionary<string, TypedValue> variables;

        public InterpreterContext()
        {
            variables = new Dictionary<string, TypedValue>();
        }

        public void SetVariable(string name, TypedValue value)
        {
            variables[name] = value;
        }

        public void SetVariable(string name, string value)
        {
            variables[name] = new TypedValue(value);
        }

        public void SetVariable(string name, int value)
        {
            variables[name] = new TypedValue(value);
        }

        public void SetVariable(string name, bool value)
        {
            variables[name] = new TypedValue(value);
        }

        public TypedValue GetVariable(string name)
        {
            return variables.ContainsKey(name) ? variables[name] : null;
        }

        public override string ToString()
        {
            var dmp = new StringBuilder();
            foreach (var pair in variables) dmp.AppendLine($"{pair.Key}={pair.Value}");
            return dmp.ToString();
        }
    }

    public class Interpreter
    {
        private ExpressionEvaluator evaluator;

        private bool IsQuiet;

        public InterpreterContext Interprete(WhileAST ast, bool quiet = false)
        {
            IsQuiet = quiet;
            evaluator = new ExpressionEvaluator(quiet);
            return Interprete(ast, new InterpreterContext());
        }

        public InterpreterContext Interprete(WhileAST ast)
        {
            evaluator = new ExpressionEvaluator();
            return Interprete(ast, new InterpreterContext());
        }

        private InterpreterContext Interprete(WhileAST ast, InterpreterContext context)
        {
            if (ast is AssignStatement assign) Interprete(assign, context);
            if (ast is SequenceStatement seq) Interprete(seq, context);
            if (ast is SkipStatement skip)
            {
                //Interprete(skip, context);
            }

            if (ast is PrintStatement print) Interprete(print, context);
            if (ast is IfStatement si) Interprete(si, context);
            if (ast is WhileStatement tantque) Interprete(tantque, context);
            return context;
        }

        private void Interprete(AssignStatement ast, InterpreterContext context)
        {
            var val = evaluator.Evaluate(ast.Value, context);
            context.SetVariable(ast.VariableName, val);
        }

        private void Interprete(PrintStatement ast, InterpreterContext context)
        {
            var val = evaluator.Evaluate(ast.Value, context);
            if (!IsQuiet) Console.WriteLine(val.StringValue);
        }

        private void Interprete(SequenceStatement ast, InterpreterContext context)
        {
            for (var i = 0; i < ast.Count; i++)
            {
                var stmt = ast.Get(i);
                Interprete(stmt, context);
            }
        }

        private void Interprete(IfStatement ast, InterpreterContext context)
        {
            var val = evaluator.Evaluate(ast.Condition, context);
            if (val.ValueType != WhileType.BOOL)
                throw new InterpreterException($"invalid condition type {ast.Condition.Dump("")}");

            if (val.BoolValue)
                Interprete(ast.ThenStmt, context);
            else
                Interprete(ast.ElseStmt, context);
        }

        private void Interprete(WhileStatement ast, InterpreterContext context)
        {
            var cond = evaluator.Evaluate(ast.Condition, context);
            if (cond.ValueType != WhileType.BOOL)
                throw new InterpreterException($"invalid condition type {ast.Condition.Dump("")}");
            while (cond.BoolValue)
            {
                Interprete(ast.BlockStmt, context);
                cond = evaluator.Evaluate(ast.Condition, context);
            }
        }
    }


    #region EXPRESSIONS

    internal class Signature
    {
        private readonly WhileType Left;
        public WhileType Result;
        private readonly WhileType Right;

        public Signature(WhileType left, WhileType right, WhileType result)
        {
            Left = left;
            Right = right;
            Result = result;
        }

        public bool Match(WhileType l, WhileType r)
        {
            return (Left == WhileType.ANY || l == Left) &&
                   (Right == WhileType.ANY || r == Right);
        }
    }

    internal class ExpressionEvaluator
    {
        private readonly Dictionary<BinaryOperator, List<Signature>> binaryOperationSignatures;

        public ExpressionEvaluator(bool quiet = false)
        {
            IsQuiet = quiet;
            binaryOperationSignatures = new Dictionary<BinaryOperator, List<Signature>>
            {
                {
                    BinaryOperator.ADD, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.INT)
                    }
                },
                {
                    BinaryOperator.SUB, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.INT)
                    }
                },
                {
                    BinaryOperator.DIVIDE, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.INT)
                    }
                },
                {
                    BinaryOperator.MULTIPLY, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.INT)
                    }
                },
                {
                    BinaryOperator.AND, new List<Signature>
                    {
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.OR, new List<Signature>
                    {
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.LESSER, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.BOOL),
                        new Signature(WhileType.STRING, WhileType.STRING, WhileType.BOOL),
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.GREATER, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.BOOL),
                        new Signature(WhileType.STRING, WhileType.STRING, WhileType.BOOL),
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.EQUALS, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.BOOL),
                        new Signature(WhileType.STRING, WhileType.STRING, WhileType.BOOL),
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.DIFFERENT, new List<Signature>
                    {
                        new Signature(WhileType.INT, WhileType.INT, WhileType.BOOL),
                        new Signature(WhileType.STRING, WhileType.STRING, WhileType.BOOL),
                        new Signature(WhileType.BOOL, WhileType.BOOL, WhileType.BOOL)
                    }
                },
                {
                    BinaryOperator.CONCAT, new List<Signature>
                    {
                        new Signature(WhileType.ANY, WhileType.ANY, WhileType.STRING)
                    }
                }
            };
        }

        private bool IsQuiet { get; }

        public WhileType CheckBinaryOperationTyping(BinaryOperator oper, WhileType left, WhileType right)
        {
            WhileType result;
            if (binaryOperationSignatures.ContainsKey(oper))
            {
                var signatures = binaryOperationSignatures[oper];
                var res = signatures.Find(sig => sig.Match(left, right));
                if (res != null)
                    result = res.Result;
                else
                    throw new InterpreterException($"invalid operation {left} {oper} {right}");
            }
            else
            {
                result = left;
            }

            return result;
        }

        public TypedValue Evaluate(Expression expr, InterpreterContext context)
        {
            if (expr is BoolConstant b) return Evaluate(b, context);
            if (expr is IntegerConstant i) return Evaluate(i, context);
            if (expr is StringConstant s) return Evaluate(s, context);
            if (expr is BinaryOperation binary) return Evaluate(binary, context);
            if (expr is Neg neg) return Evaluate(neg, context);
            if (expr is Not not) return Evaluate(not, context);
            if (expr is Variable variable) return context.GetVariable(variable.Name);
            throw new InterpreterException($"unknow expression type ({expr.GetType().Name})");
        }

        public TypedValue Evaluate(BoolConstant boolConst, InterpreterContext context)
        {
            return new TypedValue(boolConst.Value);
        }

        public TypedValue Evaluate(StringConstant stringConst, InterpreterContext context)
        {
            return new TypedValue(stringConst.Value);
        }

        public TypedValue Evaluate(IntegerConstant intConst, InterpreterContext context)
        {
            return new TypedValue(intConst.Value);
        }

        public TypedValue Evaluate(BinaryOperation operation, InterpreterContext context)
        {
            var left = Evaluate(operation.Left, context);
            var right = Evaluate(operation.Right, context);
            var resultType = CheckBinaryOperationTyping(operation.Operator, left.ValueType, right.ValueType);
            object value = null;

            switch (operation.Operator)
            {
                case BinaryOperator.ADD:
                {
                    value = left.IntValue + right.IntValue;
                    break;
                }
                case BinaryOperator.SUB:
                {
                    value = left.IntValue - right.IntValue;
                    break;
                }
                case BinaryOperator.MULTIPLY:
                {
                    value = left.IntValue * right.IntValue;
                    break;
                }
                case BinaryOperator.DIVIDE:
                {
                    value = left.IntValue / right.IntValue;
                    break;
                }
                case BinaryOperator.AND:
                {
                    value = left.BoolValue && right.BoolValue;
                    break;
                }
                case BinaryOperator.OR:
                {
                    value = left.BoolValue || right.BoolValue;
                    break;
                }
                case BinaryOperator.GREATER:
                {
                    switch (left.ValueType)
                    {
                        case WhileType.BOOL:
                        {
                            value = left.BoolValue && right.BoolValue == false;
                            break;
                        }
                        case WhileType.INT:
                        {
                            value = left.IntValue > right.IntValue;
                            break;
                        }
                        case WhileType.STRING:
                        {
                            value = left.StringValue.CompareTo(right.StringValue) == 1;
                            break;
                        }
                        default:
                        {
                            value = false;
                            break;
                        }
                    }

                    break;
                }
                case BinaryOperator.LESSER:
                {
                    switch (left.ValueType)
                    {
                        case WhileType.BOOL:
                        {
                            value = left.BoolValue == false && right.BoolValue;
                            break;
                        }
                        case WhileType.INT:
                        {
                            value = left.IntValue < right.IntValue;
                            break;
                        }
                        case WhileType.STRING:
                        {
                            value = left.StringValue.CompareTo(right.StringValue) == -1;
                            break;
                        }
                        default:
                        {
                            value = false;
                            break;
                        }
                    }

                    break;
                }
                case BinaryOperator.EQUALS:
                {
                    switch (left.ValueType)
                    {
                        case WhileType.BOOL:
                        {
                            value = left.BoolValue == right.BoolValue;
                            break;
                        }
                        case WhileType.INT:
                        {
                            value = left.IntValue == right.IntValue;
                            break;
                        }
                        case WhileType.STRING:
                        {
                            value = left.StringValue == right.StringValue;
                            break;
                        }
                        default:
                        {
                            value = false;
                            break;
                        }
                    }

                    break;
                }
                case BinaryOperator.DIFFERENT:
                {
                    switch (left.ValueType)
                    {
                        case WhileType.BOOL:
                        {
                            value = left.BoolValue != right.BoolValue;
                            break;
                        }
                        case WhileType.INT:
                        {
                            value = left.IntValue != right.IntValue;
                            break;
                        }
                        case WhileType.STRING:
                        {
                            value = left.StringValue != right.StringValue;
                            break;
                        }
                        default:
                        {
                            value = false;
                            break;
                        }
                    }

                    break;
                }
                case BinaryOperator.CONCAT:
                {
                    value = left.StringValue + right.StringValue;
                    break;
                }
                default:
                {
                    value = null;
                    break;
                }
            }

            return new TypedValue(resultType, value);
        }


        public TypedValue Evaluate(Neg neg, InterpreterContext context)
        {
            var positiveVal = Evaluate(neg.Value, context);
            if (positiveVal.ValueType != WhileType.INT)
                throw new InterpreterException($"invalid operation - {positiveVal.StringValue}");
            return new TypedValue(-positiveVal.IntValue);
        }

        public TypedValue Evaluate(Not not, InterpreterContext context)
        {
            var positiveVal = Evaluate(not.Value, context);
            if (positiveVal.ValueType != WhileType.BOOL)
                throw new InterpreterException($"invalid operation NOT {positiveVal.StringValue}");
            return new TypedValue(!positiveVal.BoolValue);
        }
    }

    #endregion
}