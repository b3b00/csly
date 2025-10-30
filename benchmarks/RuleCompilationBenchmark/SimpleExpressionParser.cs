using System;
using sly.lexer;
using sly.parser.generator;
using sly.parser;

namespace RuleCompilationBenchmark
{
    /// <summary>
    /// Simple expression parser for benchmarking
    /// Supports: numbers, +, -, *, /, parentheses
    /// </summary>
    public class SimpleExpressionParser
    {
        // Terminal rules - simple tokens
        [Production("primary : NUMBER")]
        public ExpressionNode Number(Token<ExpressionToken> number)
        {
            return new ExpressionNode(double.Parse(number.Value));
        }

        [Production("primary : LPAREN expression RPAREN")]
        public ExpressionNode ParenExpression(
            Token<ExpressionToken> lparen,
            ExpressionNode expr,
            Token<ExpressionToken> rparen)
        {
            return expr;
        }

        // Multiplicative operations
        [Production("term : primary")]
        public ExpressionNode PrimaryToTerm(ExpressionNode primary)
        {
            return primary;
        }

        [Production("term : primary MULTIPLY term")]
        public ExpressionNode Multiply(ExpressionNode left, Token<ExpressionToken> op, ExpressionNode right)
        {
            return new ExpressionNode("*", left, right);
        }

        [Production("term : primary DIVIDE term")]
        public ExpressionNode Divide(ExpressionNode left, Token<ExpressionToken> op, ExpressionNode right)
        {
            return new ExpressionNode("/", left, right);
        }

        // Additive operations
        [Production("expression : term")]
        public ExpressionNode TermToExpression(ExpressionNode term)
        {
            return term;
        }

        [Production("expression : term PLUS expression")]
        public ExpressionNode Add(ExpressionNode left, Token<ExpressionToken> op, ExpressionNode right)
        {
            return new ExpressionNode("+", left, right);
        }

        [Production("expression : term MINUS expression")]
        public ExpressionNode Subtract(ExpressionNode left, Token<ExpressionToken> op, ExpressionNode right)
        {
            return new ExpressionNode("-", left, right);
        }
    }
}

