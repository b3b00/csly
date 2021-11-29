using sly.lexer;
using sly.parser;
using sly.parser.generator;
using System;
using System.Globalization;
using System.Linq;

namespace CslyNullIssue
{
    public abstract class ExpressionParser
    {
        [Operand]
        [Production("logical_literal: OFF")]
        [Production("logical_literal: ON")]
        public string LiteralBool(Token<ExpressionToken> token)
        {
            return token.Value;
        }

        [Operand]
        [Production("primary: HEX_NUMBER")]
        public string NumericExpressionFromLiteralNumber(Token<ExpressionToken> offsetToken)
        {
            return offsetToken.Value;
        }

        [Operand]
        [Production("primary: DECIMAL_NUMBER")]
        public string NumericExpressionFromDecimalNumber(Token<ExpressionToken> offsetToken)
        {
            var text = offsetToken.Value;
            var value = double.Parse(text, CultureInfo.InvariantCulture);
            return value.ToString(CultureInfo.InvariantCulture);
        }

        [Infix((int)ExpressionToken.PLUS, Associativity.Left, 14)]
        [Infix((int)ExpressionToken.MINUS, Associativity.Left, 14)]
        [Infix((int)ExpressionToken.TIMES, Associativity.Left, 15)]
        [Infix((int)ExpressionToken.DIVIDE, Associativity.Left, 15)]
        [Infix((int)ExpressionToken.BITWISE_AND, Associativity.Left, 10)]
        [Infix((int)ExpressionToken.BITWISE_OR, Associativity.Left, 8)]
        public string NumberExpression(string lhs, Token<ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }

        [Infix((int)ExpressionToken.LOGICAL_AND, Associativity.Left, 7)]
        [Infix((int)ExpressionToken.LOGICAL_OR, Associativity.Left, 6)]
        public string LogicalExpression(string lhs, Token<ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }

        [Prefix((int)ExpressionToken.MINUS, Associativity.Right, 17)]
        public string NumericExpression(Token<ExpressionToken> _, string child)
        {
            return $"-{child}";
        }

        // We want NOT to to bind tighter than AND/OR but looser than numeric comparison operations
        [Prefix((int)ExpressionToken.NOT, Associativity.Right, 11)]
        public string LogicalExpression(Token<ExpressionToken> _, string child)
        {
            return $"(NOT {child})";
        }

        [Infix((int)ExpressionToken.COMPARISON, Associativity.Left, 12)]
        public string Comparison(string lhs, Token<ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }

        private static Parser<ExpressionToken, string> cachedParser;

        public static string Parse<T>(string expression) where T : ExpressionParser, new()
        {
            if (cachedParser == null)
            {
                var startingRule = $"{typeof(T).Name}_expressions";
                var parserInstance = new T();
                var builder = new ParserBuilder<ExpressionToken, string>();
                var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
                if (parser.IsError)
                {
                    throw new Exception($"Could not create parser. BNF is not valid. {parser.Errors[0]}");
                }
                cachedParser = parser.Result;
            }

            // To simplify an ambiguous lexer which would result from having both && and & as well as || and |, we'll
            // simplify the incoming expression by turning && into AND and || into OR:
            expression = expression.Replace("&&", " AND ");
            expression = expression.Replace("||", " OR ");

            var parseResult = cachedParser.Parse(expression);
            if (parseResult.IsError)
            {
                if (parseResult.Errors.Any())
                {
                    throw new Exception(parseResult.Errors[0].ErrorMessage);
                }
                else
                {
                    throw new Exception("unknwon error ");
                }
            }

            return parseResult.Result;
        }
    }
}