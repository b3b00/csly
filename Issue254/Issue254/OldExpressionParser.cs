using System;
using System.Globalization;
using BravoLights.Common;
using BravoLights.Common.Ast;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace Issue254.Issue254
{
    public class OldExpressionParser
    {
           [Production("logicalPrimary: OFF")]
        [Production("logicalPrimary: ON")]
        public IAstNode LiteralBool(Token<OldExpressionToken> token)
        {
            return new LiteralBoolNode(token.Value == "ON");
        }

        [Production("primary: HEX_NUMBER")]
        public IAstNode NumericExpressionFromLiteralNumber(Token<OldExpressionToken> offsetToken)
        {
            var text = offsetToken.Value;
            var num = text[2..];

            // TODO: error handling
            var value = int.Parse(num, NumberStyles.HexNumber);
            return new LiteralNumericNode(value);
        }

        [Production("primary: DECIMAL_NUMBER")]
        public IAstNode NumericExpressionFromDecimalNumber(Token<OldExpressionToken> offsetToken)
        {
            var text = offsetToken.Value;

            // TODO: error handling
            var value = double.Parse(text, CultureInfo.InvariantCulture);
            return new LiteralNumericNode(value);
        }

        [Production("numericExpression: term PLUS numericExpression")]
        [Production("numericExpression: term MINUS numericExpression")]
        [Production("term: factor TIMES term")]
        [Production("term: factor DIVIDE term")]
        public IAstNode NumberExpression(IAstNode lhs, Token<OldExpressionToken> token, IAstNode rhs)
        {
            return BinaryNumericExpression.Create(lhs, token, rhs);
        }

        [Production("numericExpression: term")]
        [Production("term: factor")]
        [Production("factor: primary")]
        [Production("logicalPrimary: comparison")]
        [Production("logicalTerm: logicalPrimary")]
        [Production("logicalExpression: logicalTerm")]
        public IAstNode Direct(IAstNode node)

        {
            return node;
        }

        [Production("logicalExpression: logicalTerm OR logicalExpression")]
        [Production("logicalTerm: logicalPrimary AND logicalTerm")]
        public IAstNode LogicalJunction(IAstNode lhs, Token<OldExpressionToken> token, IAstNode rhs)
        {
            return BooleanLogicalExpression.Create(lhs, token, rhs);
        }

        [Production("primary: MINUS [d] primary")]
        public IAstNode UnaryMinus(IAstNode child)
        {
            return new UnaryMinusExpression(child);
        }

        [Production("logicalPrimary: NOT [d] logicalPrimary")]
        public IAstNode LogicalUnary(IAstNode child)
        {
            return new NotExpression(child);
        }

        [Production("comparison: numericExpression COMPARISON numericExpression")]
        public IAstNode Comparison(IAstNode lhs, Token<OldExpressionToken> token, IAstNode rhs)
        {
            return ComparisonExpression.Create(lhs, token, rhs);
        }


        [Production("primary: LPAREN [d] numericExpression RPAREN [d]")]
        [Production("logicalPrimary: LPAREN [d] logicalExpression RPAREN [d]")]
        public IAstNode Parens(IAstNode exp)
        {
            return exp;
        }

        private static Parser<OldExpressionToken, IAstNode> cachedParser;

        public static IAstNode Parse<T>(string expression) where T : ExpressionParserBase, new()
        {
            if (cachedParser == null)
            {
                var parserInstance = new T();
                var builder = new ParserBuilder<OldExpressionToken, IAstNode>();
                var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "logicalExpression");
                if (parser.IsError)
                {
                    throw new Exception($"Could not create parser. BNF is not valid. {parser.Errors[0]}");
                }
                cachedParser = parser.Result;
            }

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
}