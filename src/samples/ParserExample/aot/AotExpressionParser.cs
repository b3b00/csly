using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace ParserExample.aot
{
    [ParserRoot("root")]
    public class AotExpressionParser
    {
        [Production("root : AotExpressionParser_expressions")]
        public double root_SimpleExpressionParserexpressions(double p0)
        {
            return p0;
        }

        [Infix("PLUS", Associativity.Right, 10)]
        public double PLUS(double left, Token<AotExpressionToken> oper, double right)
        {
            return left + right;
        }

        [Infix("MINUS", Associativity.Left, 10)]
        public double MINUS(double left, Token<AotExpressionToken> oper, double right)
        {
            return left - right;
        }

        [Infix("TIMES", Associativity.Right, 50)]
        public double TIMES(double left, Token<AotExpressionToken> oper, double right)
        {
            return left * right;
        }

        [Infix("DIVIDE", Associativity.Left, 50)]
        public double DIVIDE(double left, Token<AotExpressionToken> oper, double right)
        {
            return left / right;
        }

        [Prefix("MINUS", Associativity.Left, 100)]
        public double MINUS(Token<AotExpressionToken> oper, double value)
        {
            return -value;
        }

        [Postfix("FACTORIAL", Associativity.Left, 100)]
        public double FACTORIAL(double value, Token<AotExpressionToken> oper)
        {
            return value;
        }

        [Operand]
        [Production("primary_value : DOUBLE")]
        public double primaryvalue_DOUBLE(Token<AotExpressionToken> p0)
        {
            return p0.DoubleValue;
        }
        [Operand]
        [Production("primary_value : LPAREN AotExpressionParser_expressions RPAREN")]
        public double primaryvalue_LPAREN_SimpleExpressionParserexpressions_RPAREN(Token<AotExpressionToken> p0, double p1, Token<AotExpressionToken> p2)
        {
            return p1;
        }
    }
}