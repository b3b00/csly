using System.Linq;
using sly.parser;
using sly.parser.generator;

using Xunit;

namespace ParserTests.Issue263
{
    public class Issue263Test
    {
        [Fact]
        public void OperationCanBeParsed()
        {
            var source = "()";

            var commandParser = new Issue263Parser();
            var parserBuilder = new ParserBuilder<Issue263Token, object>();
            var buildResult = parserBuilder.BuildParser(commandParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "operation");

            Assert.True(buildResult.IsOk);

            var parser = buildResult.Result;
            var parseResult = parser.Parse(source);

            Assert.True(parseResult.IsOk);
        }

        [Theory]
        [InlineData("([", Issue263Token.LBRAC,Issue263Token.RPARA)]
        [InlineData("()()(]", Issue263Token.RBRAC,Issue263Token.RPARA)]
        public void OperationCannotBeParsedAndReturnsError(string source, Issue263Token unexpectedToken, params Issue263Token[] expectedTokens)
        {
            var commandParser = new Issue263Parser();
            var parserBuilder = new ParserBuilder<Issue263Token, object>();
            var buildResult = parserBuilder.BuildParser(commandParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "operation");

            Assert.True(buildResult.IsOk);

            var parser = buildResult.Result;
            var parseResult = parser.Parse(source);

            Assert.False(parseResult.IsOk);
            Assert.Single(parseResult.Errors);
            var error = parseResult.Errors[0];
            Assert.Equal(ErrorType.UnexpectedToken, error.ErrorType);
            Assert.IsType<UnexpectedTokenSyntaxError<Issue263Token>>(error);
            var unexpected = error as UnexpectedTokenSyntaxError<Issue263Token>;
            Assert.NotNull(unexpected);
            Assert.Equal(unexpectedToken,unexpected.UnexpectedToken.TokenID);
            Assert.Equal(expectedTokens.Length,unexpected.ExpectedTokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.True(unexpected.ExpectedTokens.Any(x => x.TokenId == expectedTokens[i]));
                //Assert.Contains(expectedTokens[i], unexpected.ExpectedTokens);
            }
        }
    }
}
