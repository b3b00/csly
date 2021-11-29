using sly.lexer;
using sly.parser;
using sly.parser.generator;
using System;
using System.Globalization;
using System.Linq;

namespace CslyNullIssue
{
    public abstract class Issue259ExpressionParser
    {
        [Operand]
        [Production("logical_literal: OFF")]
        [Production("logical_literal: ON")]
        public string LiteralBool(Token<Issue259ExpressionToken> token)
        {
            return token.Value;
        }

        [Operand]
        [Production("primary: HEX_NUMBER")]
        public string NumericExpressionFromLiteralNumber(Token<Issue259ExpressionToken> offsetToken)
        {
            return offsetToken.Value;
        }

        [Operand]
        [Production("primary: DECIMAL_NUMBER")]
        public string NumericExpressionFromDecimalNumber(Token<Issue259ExpressionToken> offsetToken)
        {
            var text = offsetToken.Value;
            var value = double.Parse(text, CultureInfo.InvariantCulture);
            return value.ToString(CultureInfo.InvariantCulture);
        }

        [Infix((int)Issue259ExpressionToken.PLUS, Associativity.Left, 14)]
        [Infix((int)Issue259ExpressionToken.MINUS, Associativity.Left, 14)]
        [Infix((int)Issue259ExpressionToken.TIMES, Associativity.Left, 15)]
        [Infix((int)Issue259ExpressionToken.DIVIDE, Associativity.Left, 15)]
        [Infix((int)Issue259ExpressionToken.BITWISE_AND, Associativity.Left, 10)]
        [Infix((int)Issue259ExpressionToken.BITWISE_OR, Associativity.Left, 8)]
        public string NumberExpression(string lhs, Token<Issue259ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }

        [Infix((int)Issue259ExpressionToken.LOGICAL_AND, Associativity.Left, 7)]
        [Infix((int)Issue259ExpressionToken.LOGICAL_OR, Associativity.Left, 6)]
        public string LogicalExpression(string lhs, Token<Issue259ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }

        [Prefix((int)Issue259ExpressionToken.MINUS, Associativity.Right, 17)]
        public string NumericExpression(Token<Issue259ExpressionToken> _, string child)
        {
            return $"-{child}";
        }

        // We want NOT to to bind tighter than AND/OR but looser than numeric comparison operations
        [Prefix((int)Issue259ExpressionToken.NOT, Associativity.Right, 11)]
        public string LogicalExpression(Token<Issue259ExpressionToken> _, string child)
        {
            return $"(NOT {child})";
        }

        [Infix((int)Issue259ExpressionToken.COMPARISON, Associativity.Left, 12)]
        public string Comparison(string lhs, Token<Issue259ExpressionToken> token, string rhs)
        {
            return $"({lhs} {token.Value} {rhs})";
        }

        private static Parser<Issue259ExpressionToken, string> cachedParser;

        public static string Parse<T>(string expression) where T : Issue259ExpressionParser, new()
        {
            if (cachedParser == null)
            {
                var startingRule = $"{typeof(T).Name}_expressions";
                var parserInstance = new T();
                var builder = new ParserBuilder<Issue259ExpressionToken, string>();
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