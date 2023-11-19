using System.Linq;
using NFluent;
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


            var startingRule = $"{nameof(Issue259Parser)}_expressions";
            var parserInstance = new Issue259Parser();
            var builder = new ParserBuilder<Issue259ExpressionToken, string>("en");
            var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, startingRule);
            Check.That(parser.IsOk).IsTrue();
            Check.That(parser.Result).IsNotNull();

            var parseResult = parser.Result.Parse(expression);

            Check.That(parseResult.IsError).IsTrue();
            Check.That(parseResult.Errors).CountIs(1);


            var error = parseResult.Errors[0];

            var unexpectedTokenError = error as UnexpectedTokenSyntaxError<Issue259ExpressionToken>;
            Check.That(unexpectedTokenError).IsNotNull();
            Check.That(unexpectedTokenError.UnexpectedToken.TokenID).IsEqualTo(Issue259ExpressionToken.COMPARISON);
            Check.That(unexpectedTokenError.UnexpectedToken.Value).IsEqualTo(">");
            var expectedTokens = unexpectedTokenError.ExpectedTokens.Select(x => x.TokenId);
            Check.That(expectedTokens).Contains(
                Issue259ExpressionToken.ON,
                Issue259ExpressionToken.OFF,
                Issue259ExpressionToken.MINUS,
                Issue259ExpressionToken.HEX_NUMBER,
                Issue259ExpressionToken.DECIMAL_NUMBER,
                Issue259ExpressionToken.LVAR,
                Issue259ExpressionToken.SIMVAR,
                Issue259ExpressionToken.LPAREN);

        }
    }
}