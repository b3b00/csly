using System;
using sly.lexer;

namespace RuleCompilationBenchmark
{
    /// <summary>
    /// Simple expression token types for benchmarking
    /// </summary>
    public enum ExpressionToken
    {
        [Double]
        NUMBER,
        [Sugar("+")]
        PLUS,
        [Sugar("-")]
        MINUS,
        [Sugar("*")]
        MULTIPLY,
        [Sugar("/")]
        DIVIDE,
        [Sugar("(")]
        LPAREN,
        [Sugar(")")]
        RPAREN,
        EOF
    }

    /// <summary>
    /// AST node for expression results
    /// </summary>
    public class ExpressionNode
    {
        public double Value { get; set; }
        public string Operator { get; set; }
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }

        public ExpressionNode(double value)
        {
            Value = value;
        }

        public ExpressionNode(string op, ExpressionNode left, ExpressionNode right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public double Evaluate()
        {
            if (Left == null && Right == null)
                return Value;

            var leftVal = Left?.Evaluate() ?? 0;
            var rightVal = Right?.Evaluate() ?? 0;

            return Operator switch
            {
                "+" => leftVal + rightVal,
                "-" => leftVal - rightVal,
                "*" => leftVal * rightVal,
                "/" => rightVal != 0 ? leftVal / rightVal : 0,
                _ => 0
            };
        }
    }
}

