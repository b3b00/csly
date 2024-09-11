using System;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public class IndexOutOfRangeParser
    {
        [Production("date_expression: INT DASH[d] INT DASH[d] INT T[d] INT COLON[d] INT COLON[d] INT Z[d] ( [PLUS|DASH] INT [SECOND|SECONDS|MINUTE|MINUTES|HOUR|HOURS|DAY|DAYS|WEEK|WEEKS|MONTH|MONTHS|YEAR|YEARS] )? ( SLASH[d] [HOUR|DAY|MONTH|YEAR] )?")]
        public Issue223OorDateExpression DateExpression(
            Token<Issue223OorIndexOutOfRangeToken> yearToken,
            Token<Issue223OorIndexOutOfRangeToken> monthToken,
            Token<Issue223OorIndexOutOfRangeToken> dayToken,
            Token<Issue223OorIndexOutOfRangeToken> hoursToken,
            Token<Issue223OorIndexOutOfRangeToken> minutesToken,
            Token<Issue223OorIndexOutOfRangeToken> secondsToken,
            ValueOption<Group<Issue223OorIndexOutOfRangeToken, Issue223OorExpression>> offsetGroup,
            ValueOption<Group<Issue223OorIndexOutOfRangeToken, Issue223OorExpression>> roundGroup
            )
        {
            var (dateOffsetMagnitude, dateOffsetKind) = ExtractDateOffset(offsetGroup);

            var dateRoundKind = Issue223OorDateRoundKind.NONE;
            roundGroup.Match(
                g =>
                {
                    dateRoundKind = Enum.Parse<Issue223OorDateRoundKind>(g.Token(0).Value, true);
                    return g;
                }, 
                () => null);

            return new Issue223OorDateExpression(
                $"{yearToken.IntValue:D4}-{monthToken.IntValue:D2}-{dayToken.IntValue:D2}T{hoursToken.IntValue:D2}:{minutesToken.IntValue:D2}:{secondsToken.IntValue:D2}Z", 
                dateOffsetMagnitude, 
                dateOffsetKind,
                dateRoundKind);
        }

        [Production(
            "date_expression: NOW ( [PLUS|DASH] INT [SECOND|SECONDS|MINUTE|MINUTES|HOUR|HOURS|DAY|DAYS|WEEK|WEEKS|MONTH|MONTHS|YEAR|YEARS] )? ( SLASH[d] [HOUR|DAY|MONTH|YEAR] )?")]
        public Issue223OorDateExpression DateExpression(
            Token<Issue223OorIndexOutOfRangeToken> nowToken,
            ValueOption<Group<Issue223OorIndexOutOfRangeToken, Issue223OorExpression>> offsetGroup,
            ValueOption<Group<Issue223OorIndexOutOfRangeToken, Issue223OorExpression>> roundGroup
        )
        {
            var (dateOffsetMagnitude, dateOffsetKind) = ExtractDateOffset(offsetGroup);

            var dateRoundKind = Issue223OorDateRoundKind.NONE;
            roundGroup.Match(
                g =>
                {
                    dateRoundKind = Enum.Parse<Issue223OorDateRoundKind>(g.Token(0).Value, true);
                    return g;
                }, 
                () => null);

            return new Issue223OorDateExpression(nowToken.Value, dateOffsetMagnitude, dateOffsetKind, dateRoundKind);
        }

        [Production("numeric_expression: INT")]
        [Production("numeric_expression: DOUBLE")]
        public Issue223OorValueExpression NumericExpression(Token<Issue223OorIndexOutOfRangeToken> valueToken)
        {
            return new Issue223OorValueExpression(valueToken.Value);
        }

        [Production("numeric_expression: DASH numeric_expression")]
        [Production("numeric_expression: DASH numeric_expression")]
        public Issue223OorValueExpression NumericExpression(
            Token<Issue223OorIndexOutOfRangeToken> DASHToken,
            Issue223OorValueExpression numericExpression)
        {
            return new Issue223OorValueExpression("-" + numericExpression.Value);
        }

        [Production("value_expression: VALUE")]
        // [Production("value_expression: STRING")]
        public Issue223OorValueExpression ValueExpression(Token<Issue223OorIndexOutOfRangeToken> valueToken)
        {
            return valueToken.TokenID switch
            {
                Issue223OorIndexOutOfRangeToken.STRING => new Issue223OorValueExpression(valueToken.StringWithoutQuotes),
                _ => new Issue223OorValueExpression(valueToken.Value)
            };
        }

        [Production("value_expression: numeric_expression")]
        [Production("value_expression: date_expression")]
        public Issue223OorExpression ValueExpression(Issue223OorValueExpression issue223OorValueExpression) => issue223OorValueExpression;

        [Production("primary_expression: value_expression")]
        public Issue223OorExpression PrimaryExpression(Issue223OorValueExpression issue223OorValueExpression) => issue223OorValueExpression;

        [Production("primary_expression: VALUE COLON primary_expression")]
        public Issue223OorFieldValueExpression PrimaryExpression(
            Token<Issue223OorIndexOutOfRangeToken> fieldNameToken,
            Token<Issue223OorIndexOutOfRangeToken> colonToken,
            Issue223OorExpression issue223OorExpression)
        {
            if (Enum.TryParse<Issue223OorFields>(fieldNameToken.Value, true, out var fieldName))
            {
                return new Issue223OorFieldValueExpression(
                    fieldName,
                    issue223OorExpression
                );
            }

            return new Issue223OorFieldValueExpression(
                null,
                new Issue223OorValueExpression($"{fieldNameToken.Value}:{issue223OorExpression}")
            );
        }

        [Production("primary_expression: LPAREN expression RPAREN")]
        public Issue223OorExpression PrimaryExpression(
            Token<Issue223OorIndexOutOfRangeToken> lParenToken,
            Issue223OorExpression issue223OorExpression,
            Token<Issue223OorIndexOutOfRangeToken> rParenToken
        )
        {
            return new Issue223OorGroupExpression(issue223OorExpression);
        }

        [Production("primary_expression: LBRACKET value_expression TO[d] value_expression RBRACKET")]
        public Issue223OorExpression PrimaryExpression(
            Token<Issue223OorIndexOutOfRangeToken> lBracketToken,
            Issue223OorValueExpression from,
            Issue223OorValueExpression to,
            Token<Issue223OorIndexOutOfRangeToken> rParenToken
        )
        {
            return new Issue223OorRangeExpression(from, to);
        }

        [Production("secondary_expression: primary_expression")]
        public Issue223OorExpression SecondaryExpression(Issue223OorExpression valueIssue223OorExpression) => valueIssue223OorExpression;

        [Production("secondary_expression: NOT secondary_expression")]
        [Production("secondary_expression: PLUS secondary_expression")]
        [Production("secondary_expression: DASH secondary_expression")]
        public Issue223OorExpression SecondaryExpression(
            Token<Issue223OorIndexOutOfRangeToken> unaryOperatorToken,
            Issue223OorExpression primaryIssue223OorExpression)
        {
            var unaryOperator = Enum.Parse<Issue223OorUnaryOperator>(unaryOperatorToken.TokenID.ToString(), true);
            return new Issue223OorUnaryExpression(unaryOperator, primaryIssue223OorExpression);
        }

        [Production("secondary_expression: primary_expression CARET INT")]
        public Issue223OorExpression SecondaryExpression(
            Issue223OorExpression primaryIssue223OorExpression,
            Token<Issue223OorIndexOutOfRangeToken> caretToken,
            Token<Issue223OorIndexOutOfRangeToken> intBoostToken)
        {
            return new Issue223OorBoostExpression(primaryIssue223OorExpression, intBoostToken.IntValue);
        }

        [Production("tertiary_expression: secondary_expression")]
        public Issue223OorExpression TertiaryExpression(Issue223OorExpression valueIssue223OorExpression) => valueIssue223OorExpression;

        [Production("tertiary_expression: secondary_expression tertiary_expression")]
        public BinaryIssue223OorExpression TertiaryExpression(
            Issue223OorExpression leftIssue223OorExpression,
            Issue223OorExpression rightIssue223OorExpression)
        {
            return new BinaryIssue223OorExpression(leftIssue223OorExpression, Issue223OorBinaryOperator.AND, rightIssue223OorExpression);
        }

        [Production("tertiary_expression: secondary_expression AND tertiary_expression")]
        [Production("tertiary_expression: secondary_expression OR tertiary_expression")]
        public BinaryIssue223OorExpression BinaryExpression(
            Issue223OorExpression leftIssue223OorExpression,
            Token<Issue223OorIndexOutOfRangeToken> binaryOperatorToken,
            Issue223OorExpression rightIssue223OorExpression)
        {
            var binaryOperator = Enum.Parse<Issue223OorBinaryOperator>(binaryOperatorToken.Value, true);
            return new BinaryIssue223OorExpression(leftIssue223OorExpression, binaryOperator, rightIssue223OorExpression);
        }

        [Production("expression: tertiary_expression")]
        public Issue223OorExpression Expression(Issue223OorExpression valueIssue223OorExpression) => valueIssue223OorExpression;
        
        private static (int, Issue223OorDateOffsetKind dateOffsetKind) ExtractDateOffset(ValueOption<Group<Issue223OorIndexOutOfRangeToken, Issue223OorExpression>> offsetGroup)
        {
            var dateOffsetMagnitude = 0;
            var dateOffsetKind = Issue223OorDateOffsetKind.SECOND;

            if (offsetGroup.IsSome)
            {
                offsetGroup.Match(
                    g =>
                    {
                        dateOffsetMagnitude = g.Token(1).IntValue;

                        if (g.Token(0).TokenID == Issue223OorIndexOutOfRangeToken.DASH)
                            dateOffsetMagnitude *= -1;

                        switch (g.Token(2).TokenID)
                        {
                            case Issue223OorIndexOutOfRangeToken.SECOND:
                            case Issue223OorIndexOutOfRangeToken.SECONDS:
                                dateOffsetKind = Issue223OorDateOffsetKind.SECOND;
                                break;
                            case Issue223OorIndexOutOfRangeToken.MINUTE:
                            case Issue223OorIndexOutOfRangeToken.MINUTES:
                                dateOffsetKind = Issue223OorDateOffsetKind.MINUTE;
                                break;
                            case Issue223OorIndexOutOfRangeToken.HOUR:
                            case Issue223OorIndexOutOfRangeToken.HOURS:
                                dateOffsetKind = Issue223OorDateOffsetKind.HOUR;
                                break;
                            case Issue223OorIndexOutOfRangeToken.DAY:
                            case Issue223OorIndexOutOfRangeToken.DAYS:
                                dateOffsetKind = Issue223OorDateOffsetKind.DAY;
                                break;
                            case Issue223OorIndexOutOfRangeToken.WEEK:
                            case Issue223OorIndexOutOfRangeToken.WEEKS:
                                dateOffsetKind = Issue223OorDateOffsetKind.WEEK;
                                break;
                            case Issue223OorIndexOutOfRangeToken.MONTH:
                            case Issue223OorIndexOutOfRangeToken.MONTHS:
                                dateOffsetKind = Issue223OorDateOffsetKind.MONTH;
                                break;
                            case Issue223OorIndexOutOfRangeToken.YEAR:
                            case Issue223OorIndexOutOfRangeToken.YEARS:
                                dateOffsetKind = Issue223OorDateOffsetKind.YEAR;
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
