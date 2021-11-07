using System;
using System.Globalization;
using BravoLights.Common.Ast;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace BravoLights.Common
{
    #pragma warning disable CA1822 // Mark members as static
    public abstract class ExpressionParserBase
    {
        [Operand]
        [Production("logical_literal: OFF")]
        [Production("logical_literal: ON")]
        public IAstNode LiteralBool(Token<ExpressionToken> token)
        {
            return new LiteralBoolNode(token.Value == "ON");
        }

        [Operand]
        [Production("primary: HEX_NUMBER")]
        public IAstNode NumericExpressionFromLiteralNumber(Token<ExpressionToken> offsetToken)
        {
            var text = offsetToken.Value;
            var num = text[2..];
            var value = int.Parse(num, NumberStyles.HexNumber);
            return new LiteralNumericNode(value);
        }

        [Operand]
        [Production("primary: DECIMAL_NUMBER")]
        public IAstNode NumericExpressionFromDecimalNumber(Token<ExpressionToken> offsetToken)
        {
            var text = offsetToken.Value;
            var value = double.Parse(text, CultureInfo.InvariantCulture);
            return new LiteralNumericNode(value);
        }

        [Infix((int)ExpressionToken.PLUS, Associativity.Left, 14)]
        [Infix((int)ExpressionToken.MINUS, Associativity.Left, 14)]
        [Infix((int)ExpressionToken.TIMES, Associativity.Left, 15)]
        [Infix((int)ExpressionToken.DIVIDE, Associativity.Left, 15)]
        [Infix((int)ExpressionToken.BITWISE_AND, Associativity.Left, 10)]
        [Infix((int)ExpressionToken.BITWISE_OR, Associativity.Left, 8)]
        public IAstNode NumberExpression(IAstNode lhs, Token<ExpressionToken> token, IAstNode rhs)
        {
            return BinaryNumericExpression.Create(lhs, token, rhs);
        }

        [Infix((int)ExpressionToken.LOGICAL_AND, Associativity.Left, 7)]
        [Infix((int)ExpressionToken.LOGICAL_OR, Associativity.Left, 6)]
        public IAstNode LogicalExpression(IAstNode lhs, Token<ExpressionToken> token, IAstNode rhs)
        {
            return BooleanLogicalExpression.Create(lhs, token, rhs);
        }

        [Prefix((int)ExpressionToken.MINUS, Associativity.Right, 17)]
        public IAstNode NumericExpression(Token<ExpressionToken> _, IAstNode child)
        {
            return new UnaryMinusExpression(child);
        }

        // We want NOT to to bind tighter than AND/OR but looser than numeric comparison operations
        [Prefix((int)ExpressionToken.NOT, Associativity.Right, 11)]
        public IAstNode LogicalExpression(Token<ExpressionToken> _, IAstNode child)
        {
            return new NotExpression(child);
        }


        [Infix((int)ExpressionToken.COMPARISON, Associativity.Left, 12)]
        public IAstNode Comparison(IAstNode lhs, Token<ExpressionToken> token, IAstNode rhs)
        {
            return ComparisonExpression.Create(lhs, token, rhs);
        }

        private static Parser<ExpressionToken, IAstNode> cachedParser;

        public static IAstNode Parse<T>(string expression) where T : ExpressionParserBase, new()
        {
            if (cachedParser == null)
            {
                var startingRule = $"{typeof(T).Name}_expressions";
                var parserInstance = new T();
                var builder = new ParserBuilder<ExpressionToken, IAstNode>();
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
                return new ErrorNode
                {
                    ErrorText = parseResult.Errors[0].ErrorMessage
                };
            }

            return parseResult.Result;
        }
    }
    #pragma warning restore CA1822 // Mark members as static
}

