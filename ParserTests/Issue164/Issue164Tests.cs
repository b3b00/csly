using System.Linq;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue164
{
    public class Issue164Tests
    {
        private static Parser<Test164Lexer, int> BuildParser()
        {
            var StartingRule = $"root";
            var parserInstance = new Test164Parser();
            var builder = new ParserBuilder<Test164Lexer, int>();
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
            Assert.IsType<UnexpectedTokenSyntaxError<Test164Lexer>>(error);
            var unexpectedTokenError = error as UnexpectedTokenSyntaxError<Test164Lexer>;
            Assert.NotNull(unexpectedTokenError);
            Assert.NotNull(unexpectedTokenError.ExpectedTokens);
            Assert.NotEmpty(unexpectedTokenError.ExpectedTokens);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == Test164Lexer.PLUS);
            ;
        }
    }
}