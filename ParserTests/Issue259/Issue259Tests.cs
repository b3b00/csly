using System;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue259
{
    public class Issue259Tests
    {
        [Fact]
        public static void Issue259Test()
        {
            var expression = "1 < 2 AND 3 <= 4 AND 5 == 6 AND 7 >= 8 AND 9 > 10 AND 11 != 12 AND 13 <> 14";
            
            
            var startingRule = $"{typeof(Issue259Parser).Name}_expressions";
            var parserInstance = new Issue259Parser();
            var builder = new ParserBuilder<Issue259ExpressionToken, string>("en");
            var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Assert.True(parser.IsOk);
            Assert.NotNull(parser.Result);
            
            var parseResult = parser.Result.Parse(expression);
            
            Assert.True(parseResult.IsError);
            Assert.Single(parseResult.Errors);

            var error = parseResult.Errors[0];

            var unexpectedTokenError = error as UnexpectedTokenSyntaxError<Issue259ExpressionToken>;
            Assert.NotNull(unexpectedTokenError);
            Assert.Equal(Issue259ExpressionToken.COMPARISON, unexpectedTokenError.UnexpectedToken.TokenID);
            Assert.Equal(">",unexpectedTokenError.UnexpectedToken.Value);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Issue259ExpressionToken.ON);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Issue259ExpressionToken.OFF);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Issue259ExpressionToken.MINUS);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Issue259ExpressionToken.HEX_NUMBER);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Issue259ExpressionToken.DECIMAL_NUMBER);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Issue259ExpressionToken.LVAR);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Issue259ExpressionToken.SIMVAR);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Issue259ExpressionToken.LPAREN);
            
        }
    }
}