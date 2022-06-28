using System.Linq;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue164
{
    public class Issue164OptionTests
    {
        private static Parser<TestOption164Lexer, int> BuildParser()
        {
            var StartingRule = $"root";
            var parserInstance = new TestOption164Parser();
            var builder = new ParserBuilder<TestOption164Lexer, int>();
            var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Assert.True(parser.IsOk);
            Assert.NotNull(parser.Result);
            return parser.Result;
        }

        [Fact]
        public static void TestOk()
        {
            var parser = BuildParser();
            var result = parser.Parse("2 + 2");
            Assert.True(result.IsOk);
            Assert.Equal(4,result.Result);
        }
        
        [Fact]
        public static void TestErrorMessage()
        {
            var parser = BuildParser();
            var result = parser.Parse("2 ( 2");
            Assert.True(result.IsError);
            var errors = result.Errors;
            Assert.Single(errors);
            var error = errors.First();
            Assert.IsType<UnexpectedTokenSyntaxError<TestOption164Lexer>>(error);
            var unexpectedTokenError = error as UnexpectedTokenSyntaxError<TestOption164Lexer>;
            Assert.NotNull(unexpectedTokenError);
            Assert.NotNull(unexpectedTokenError.ExpectedTokens);
            Assert.NotEmpty(unexpectedTokenError.ExpectedTokens);
            Assert.Equal(4,unexpectedTokenError.ExpectedTokens.Count);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x.TokenId == TestOption164Lexer.PLUS);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x.TokenId == TestOption164Lexer.MINUS);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x.TokenId == TestOption164Lexer.TIMES);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x.TokenId == TestOption164Lexer.DIVIDE);
            ;
        }
    }
}