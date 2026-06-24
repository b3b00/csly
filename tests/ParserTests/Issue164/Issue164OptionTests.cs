using System.Linq;
using NFluent;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue164
{
    public class Issue164OptionTests
    {
        private static Parser<TestOption164Lexer, int> BuildParser(ParserType parserType)
        {
            var StartingRule = $"root";
            var parserInstance = new TestOption164Parser();
            var builder = new ParserBuilder<TestOption164Lexer, int>();
            var parser = builder.BuildParser(parserInstance, parserType, StartingRule);
            Check.That(parser).IsOk();
            return parser.Result;
        }

        [Fact]
        public static void TestOk()
        {
            var parser = BuildParser(ParserType.EBNF_LL_RECURSIVE_DESCENT);
            var result = parser.Parse("2 + 2");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo(4);
        }
        
        
        
        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public static void TestErrorMessage(ParserType parserType)
        {
            var parser = BuildParser(parserType);
            var result = parser.Parse("2 ( 2");
            Check.That(result.IsError).IsTrue();
            var errors = result.Errors;
            Check.That(errors).CountIs(1);
            var error = errors.First();
            Check.That(error).IsInstanceOf<UnexpectedTokenSyntaxError<TestOption164Lexer>>();
            var unexpectedTokenError = error as UnexpectedTokenSyntaxError<TestOption164Lexer>;
            Check.That(unexpectedTokenError).IsNotNull();
            Check.That(unexpectedTokenError.ExpectedTokens).IsNotNull();
            Check.That(unexpectedTokenError.ExpectedTokens).CountIs(4);
            Check.That(unexpectedTokenError.ExpectedTokens.Select(x => x.TokenId))
                .Contains(TestOption164Lexer.PLUS, TestOption164Lexer.MINUS, TestOption164Lexer.TIMES,
                    TestOption164Lexer.DIVIDE);
        }
    }
}