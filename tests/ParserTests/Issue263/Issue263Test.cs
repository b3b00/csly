using System.Linq;
using NFluent;
using sly.parser;
using sly.parser.generator;

using Xunit;

namespace ParserTests.Issue263
{
    public class Issue263Test
    {
        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void OperationCanBeParsed(ParserType parserType)
        {
            var source = "()";

            var commandParser = new Issue263Parser();
            var parserBuilder = new ParserBuilder<Issue263Token, object>();
            var buildResult = parserBuilder.BuildParser(commandParser, parserType, "operation");

            Check.That(buildResult.IsOk).IsTrue();

            var parser = buildResult.Result;
            var parseResult = parser.Parse(source);

            Check.That(parseResult.IsOk).IsTrue();
        }

        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT,"([", Issue263Token.LBRAC,Issue263Token.RPARA)]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT, "()()(]", Issue263Token.RBRAC,Issue263Token.RPARA)]
        [InlineData(ParserType.EBNF_LL_STACK,"([", Issue263Token.LBRAC,Issue263Token.RPARA)]
        [InlineData(ParserType.EBNF_LL_STACK, "()()(]", Issue263Token.RBRAC,Issue263Token.RPARA)]
        public void OperationCannotBeParsedAndReturnsError(ParserType parserType, string source, Issue263Token unexpectedToken, params Issue263Token[] expectedTokens)
        {
            var commandParser = new Issue263Parser();
            var parserBuilder = new ParserBuilder<Issue263Token, object>();
            var buildResult = parserBuilder.BuildParser(commandParser, parserType, "operation");

            Check.That(buildResult.IsOk).IsTrue();

            var parser = buildResult.Result;
            var parseResult = parser.Parse(source);

            Check.That(parseResult.IsOk).IsFalse();
            Check.That(parseResult.Errors).CountIs(1);
            var error = parseResult.Errors[0];
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
            Check.That(error).IsInstanceOf<UnexpectedTokenSyntaxError<Issue263Token>>();
            var unexpected = error as UnexpectedTokenSyntaxError<Issue263Token>;
            Check.That(unexpected).IsNotNull();
            Check.That(unexpected.UnexpectedToken.TokenID).IsEqualTo(unexpectedToken);
            Check.That(unexpected.ExpectedTokens).CountIs(expectedTokens.Length);
            Check.That(unexpected.ExpectedTokens.Select(x => x.TokenId)).Contains(expectedTokens);
        }
    }
}
