using System;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue223_EarlyEos
{
    public class EarlyEosParser
    {
        [Production("primary_expression: VALUE COLON primary_expression")]
        public FieldValueExpression PrimaryExpression(
            Token<EarlyEosToken> fieldNameToken,
            Token<EarlyEosToken> colonToken,
            Expression expression)
        {
            if (Enum.TryParse<Fields>(fieldNameToken.Value, true, out var fieldName))
            {
                return new FieldValueExpression(
                    fieldName,
                    expression
                );
            }

            return new FieldValueExpression(
                null,
                new ValueExpression($"{fieldNameToken.Value}:{expression}")
            );
        }

        [Production("primary_expression: LPAREN SPACE? expression SPACE? RPAREN")]
        public GroupExpression PrimaryExpression(
            Token<EarlyEosToken> lParenToken,
            Token<EarlyEosToken> lSpaceToken,
            Expression expression,
            Token<EarlyEosToken> rSpaceToken,
            Token<EarlyEosToken> rParenToken
            )
        {
            return new GroupExpression(expression);
        }

        [Production("primary_expression: INT")]
        [Production("primary_expression: DOUBLE")]
        [Production("primary_expression: VALUE")]
        [Production("primary_expression: QUOTED_VALUE")]
        public ValueExpression PrimaryExpression(Token<EarlyEosToken> valueToken)
        {
            switch (valueToken.TokenID)
            {
                case EarlyEosToken.INT:
                case EarlyEosToken.DOUBLE:
                case EarlyEosToken.VALUE:
                    return new ValueExpression(valueToken.Value);
                case EarlyEosToken.QUOTED_VALUE:
                    return new ValueExpression(
                        valueToken.Value
                            .TrimStart('\'')
                            .TrimStart('"')
                            .TrimEnd('\'')
                            .TrimEnd('"')
                    );
                default:
                    throw new Exception($"Invalid value token {valueToken.TokenID}");
            }
        }

        [Production("secondary_expression: primary_expression")]
        public Expression SecondaryExpression(Expression valueExpression) => valueExpression;

        [Production("secondary_expression: NOT SPACE[d] secondary_expression")]
        [Production("secondary_expression: PLUS secondary_expression")]
        [Production("secondary_expression: MINUS secondary_expression")]
        public Expression SecondaryExpression(
            Token<EarlyEosToken> unaryOperatorToken,
            Expression unaryExpression)
        {
            var unaryOperator = Enum.Parse<UnaryOperator>(unaryOperatorToken.Value, true);
            return new UnaryExpression(unaryOperator, unaryExpression);
        }

        [Production("tertiary_expression: secondary_expression")]
        public Expression TertiaryExpression(Expression valueExpression) => valueExpression;

        [Production("tertiary_expression: secondary_expression SPACE[d] tertiary_expression")]
        public BinaryExpression TertiaryExpression(
            Expression leftExpression,
            Expression rightExpression)
        {
            return new BinaryExpression(leftExpression, BinaryOperator.AND, rightExpression);
        }

        [Production("tertiary_expression: secondary_expression SPACE[d] AND SPACE[d] tertiary_expression")]
        [Production("tertiary_expression: secondary_expression SPACE[d] OR SPACE[d] tertiary_expression")]
        public BinaryExpression BinaryExpression(
            Expression leftExpression,
            Token<EarlyEosToken> binaryOperatorToken,
            Expression rightExpression)
        {
            var binaryOperator = Enum.Parse<BinaryOperator>(binaryOperatorToken.Value, true);
            return new BinaryExpression(leftExpression, binaryOperator, rightExpression);
        }

        [Production("expression: tertiary_expression")]
        public Expression Expression(Expression valueExpression) => valueExpression;
    }}