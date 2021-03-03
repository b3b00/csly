using System.Linq;
using expressionparser;
using simpleExpressionParser;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue164
{
    public class Issue164Tests
    {
        private static Parser<ExpressionToken, double> BuildParser()
        {   
            var StartingRule = $"{typeof(SimpleExpressionParser).Name}_expressions";
            var parserInstance = new SimpleExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, double>();
            var pBuild = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Assert.True(pBuild.IsOk);
            Assert.NotNull(pBuild.Result);
            return pBuild.Result;
        }
        
        

        [Fact]
        public static void TestOk()
        {
            var parser = BuildParser();
            var result = parser.Parse("2 + 2");
            Assert.True(result.IsOk);
            Assert.Equal(4.0,result.Result);
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
            Assert.IsType<UnexpectedTokenSyntaxError<ExpressionToken>>(error);
            var unexpectedTokenError = error as UnexpectedTokenSyntaxError<ExpressionToken>;
            Assert.NotNull(unexpectedTokenError);
            Assert.NotNull(unexpectedTokenError.ExpectedTokens);
            Assert.NotEmpty(unexpectedTokenError.ExpectedTokens);
            Assert.Equal(5,unexpectedTokenError.ExpectedTokens.Count);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == ExpressionToken.PLUS);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == ExpressionToken.MINUS);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == ExpressionToken.TIMES);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == ExpressionToken.DIVIDE);
            Assert.Contains(unexpectedTokenError.ExpectedTokens, x => x == ExpressionToken.FACTORIAL);
            ;
        }
    }
}