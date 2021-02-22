using System;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class IndexOutOfRangeParser
    {
        [Production("date_expression: INT DASH[d] INT DASH[d] INT T[d] INT COLON[d] INT COLON[d] INT Z[d] ( [PLUS|DASH] INT [SECOND|SECONDS|MINUTE|MINUTES|HOUR|HOURS|DAY|DAYS|WEEK|WEEKS|MONTH|MONTHS|YEAR|YEARS] )? ( SLASH[d] [HOUR|DAY|MONTH|YEAR] )?")]
        public DateExpression DateExpression(
            Token<IndexOutOfRangeToken> yearToken,
            Token<IndexOutOfRangeToken> monthToken,
            Token<IndexOutOfRangeToken> dayToken,
            Token<IndexOutOfRangeToken> hoursToken,
            Token<IndexOutOfRangeToken> minutesToken,
            Token<IndexOutOfRangeToken> secondsToken,
            ValueOption<Group<IndexOutOfRangeToken, Expression>> offsetGroup,
            ValueOption<Group<IndexOutOfRangeToken, Expression>> roundGroup
            )
        {
            var (dateOffsetMagnitude, dateOffsetKind) = ExtractDateOffset(offsetGroup);

            var dateRoundKind = DateRoundKind.NONE;
            roundGroup.Match(
                g =>
                {
                    dateRoundKind = Enum.Parse<DateRoundKind>(g.Token(0).Value, true);
                    return g;
                }, 
                () => null);

            return new DateExpression(
                $"{yearToken.IntValue:D4}-{monthToken.IntValue:D2}-{dayToken.IntValue:D2}T{hoursToken.IntValue:D2}:{minutesToken.IntValue:D2}:{secondsToken.IntValue:D2}Z", 
                dateOffsetMagnitude, 
                dateOffsetKind,
                dateRoundKind);
        }

        [Production(
            "date_expression: NOW ( [PLUS|DASH] INT [SECOND|SECONDS|MINUTE|MINUTES|HOUR|HOURS|DAY|DAYS|WEEK|WEEKS|MONTH|MONTHS|YEAR|YEARS] )? ( SLASH[d] [HOUR|DAY|MONTH|YEAR] )?")]
        public DateExpression DateExpression(
            Token<IndexOutOfRangeToken> nowToken,
            ValueOption<Group<IndexOutOfRangeToken, Expression>> offsetGroup,
            ValueOption<Group<IndexOutOfRangeToken, Expression>> roundGroup
        )
        {
            var (dateOffsetMagnitude, dateOffsetKind) = ExtractDateOffset(offsetGroup);

            var dateRoundKind = DateRoundKind.NONE;
            roundGroup.Match(
                g =>
                {
                    dateRoundKind = Enum.Parse<DateRoundKind>(g.Token(0).Value, true);
                    return g;
                }, 
                () => null);

            return new DateExpression(nowToken.Value, dateOffsetMagnitude, dateOffsetKind, dateRoundKind);
        }

        [Production("numeric_expression: INT")]
        [Production("numeric_expression: DOUBLE")]
        public ValueExpression NumericExpression(Token<IndexOutOfRangeToken> valueToken)
        {
            return new ValueExpression(valueToken.Value);
        }

        [Production("numeric_expression: DASH numeric_expression")]
        [Production("numeric_expression: DASH numeric_expression")]
        public ValueExpression NumericExpression(
            Token<IndexOutOfRangeToken> DASHToken,
            ValueExpression numericExpression)
        {
            return new ValueExpression("-" + numericExpression.Value);
        }

        [Production("value_expression: VALUE")]
        // [Production("value_expression: STRING")]
        public ValueExpression ValueExpression(Token<IndexOutOfRangeToken> valueToken)
        {
            return valueToken.TokenID switch
            {
                IndexOutOfRangeToken.STRING => new ValueExpression(valueToken.StringWithoutQuotes),
                _ => new ValueExpression(valueToken.Value)
            };
        }

        [Production("value_expression: numeric_expression")]
        [Production("value_expression: date_expression")]
        public Expression ValueExpression(ValueExpression valueExpression) => valueExpression;

        [Production("primary_expression: value_expression")]
        public Expression PrimaryExpression(ValueExpression valueExpression) => valueExpression;

        [Production("primary_expression: VALUE COLON primary_expression")]
        public FieldValueExpression PrimaryExpression(
            Token<IndexOutOfRangeToken> fieldNameToken,
            Token<IndexOutOfRangeToken> colonToken,
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

        [Production("primary_expression: LPAREN expression RPAREN")]
        public Expression PrimaryExpression(
            Token<IndexOutOfRangeToken> lParenToken,
            Expression expression,
            Token<IndexOutOfRangeToken> rParenToken
        )
        {
            return new GroupExpression(expression);
        }

        [Production("primary_expression: LBRACKET value_expression TO[d] value_expression RBRACKET")]
        public Expression PrimaryExpression(
            Token<IndexOutOfRangeToken> lBracketToken,
            ValueExpression from,
            ValueExpression to,
            Token<IndexOutOfRangeToken> rParenToken
        )
        {
            return new RangeExpression(from, to);
        }

        [Production("secondary_expression: primary_expression")]
        public Expression SecondaryExpression(Expression valueExpression) => valueExpression;

        [Production("secondary_expression: NOT secondary_expression")]
        [Production("secondary_expression: PLUS secondary_expression")]
        [Production("secondary_expression: DASH secondary_expression")]
        public Expression SecondaryExpression(
            Token<IndexOutOfRangeToken> unaryOperatorToken,
            Expression primaryExpression)
        {
            var unaryOperator = Enum.Parse<UnaryOperator>(unaryOperatorToken.TokenID.ToString(), true);
            return new UnaryExpression(unaryOperator, primaryExpression);
        }

        [Production("secondary_expression: primary_expression CARET INT")]
        public Expression SecondaryExpression(
            Expression primaryExpression,
            Token<IndexOutOfRangeToken> caretToken,
            Token<IndexOutOfRangeToken> intBoostToken)
        {
            return new BoostExpression(primaryExpression, intBoostToken.IntValue);
        }

        [Production("tertiary_expression: secondary_expression")]
        public Expression TertiaryExpression(Expression valueExpression) => valueExpression;

        [Production("tertiary_expression: secondary_expression tertiary_expression")]
        public BinaryExpression TertiaryExpression(
            Expression leftExpression,
            Expression rightExpression)
        {
            return new BinaryExpression(leftExpression, BinaryOperator.AND, rightExpression);
        }

        [Production("tertiary_expression: secondary_expression AND tertiary_expression")]
        [Production("tertiary_expression: secondary_expression OR tertiary_expression")]
        public BinaryExpression BinaryExpression(
            Expression leftExpression,
            Token<IndexOutOfRangeToken> binaryOperatorToken,
            Expression rightExpression)
        {
            var binaryOperator = Enum.Parse<BinaryOperator>(binaryOperatorToken.Value, true);
            return new BinaryExpression(leftExpression, binaryOperator, rightExpression);
        }

        [Production("expression: tertiary_expression")]
        public Expression Expression(Expression valueExpression) => valueExpression;
        
        private static (int, DateOffsetKind dateOffsetKind) ExtractDateOffset(ValueOption<Group<IndexOutOfRangeToken, Expression>> offsetGroup)
        {
            var dateOffsetMagnitude = 0;
            var dateOffsetKind = DateOffsetKind.SECOND;

            if (offsetGroup.IsSome)
            {
                offsetGroup.Match(
                    g =>
                    {
                        dateOffsetMagnitude = g.Token(1).IntValue;

                        if (g.Token(0).TokenID == IndexOutOfRangeToken.DASH)
                            dateOffsetMagnitude *= -1;

                        switch (g.Token(2).TokenID)
                        {
                            case IndexOutOfRangeToken.SECOND:
                            case IndexOutOfRangeToken.SECONDS:
                                dateOffsetKind = DateOffsetKind.SECOND;
                                break;
                            case IndexOutOfRangeToken.MINUTE:
                            case IndexOutOfRangeToken.MINUTES:
                                dateOffsetKind = DateOffsetKind.MINUTE;
                                break;
                            case IndexOutOfRangeToken.HOUR:
                            case IndexOutOfRangeToken.HOURS:
                                dateOffsetKind = DateOffsetKind.HOUR;
                                break;
                            case IndexOutOfRangeToken.DAY:
                            case IndexOutOfRangeToken.DAYS:
                                dateOffsetKind = DateOffsetKind.DAY;
                                break;
                            case IndexOutOfRangeToken.WEEK:
                            case IndexOutOfRangeToken.WEEKS:
                                dateOffsetKind = DateOffsetKind.WEEK;
                                break;
                            case IndexOutOfRangeToken.MONTH:
                            case IndexOutOfRangeToken.MONTHS:
                                dateOffsetKind = DateOffsetKind.MONTH;
                                break;
                            case IndexOutOfRangeToken.YEAR:
                            case IndexOutOfRangeToken.YEARS:
                                dateOffsetKind = DateOffsetKind.YEAR;
                                break;
                        }

                        return g;
                    },
                    () => null
                );
            }

            return (dateOffsetMagnitude, dateOffsetKind);
        }
    }
}
