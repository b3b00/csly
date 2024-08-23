using System.Linq;
using expressionparser;
using NFluent;
using simpleExpressionParser;
using sly.parser;
using sly.parser.generator;
using Xunit;
using ExpressionToken = simpleExpressionParser.ExpressionToken;

namespace ParserTests.Issue164
{
    public class Issue164Tests
    {
        private static Parser<ExpressionToken, double> BuildParser()
        {   
            var StartingRule = $"{nameof(SimpleExpressionParser)}_expressions";
            var parserInstance = new SimpleExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, double>();
            var pBuild = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Check.That(pBuild).IsOk();
            return pBuild.Result;
        }
        
        

        [Fact]
        public static void TestOk()
        {
            var parser = BuildParser();
            var result = parser.Parse("2 + 2");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo(4.0);
        }
        
        [Fact]
        public static void TestErrorMessage()
        {
            var parser = BuildParser();
            var result = parser.Parse("2 ( 2");
            Check.That(result).Not.IsOkParsing();
            var errors = result.Errors;
            Check.That(errors).IsSingle();
            var error = errors.First();
            Check.That(error).IsInstanceOf<UnexpectedTokenSyntaxError<ExpressionToken>>();
            var unexpectedTokenError = error as UnexpectedTokenSyntaxError<ExpressionToken>;
            Check.That(unexpectedTokenError).IsNotNull();
            Check.That(unexpectedTokenError.ExpectedTokens).IsNotNull();
            Check.That(unexpectedTokenError.ExpectedTokens).Not.IsEmpty();
            Check.That(unexpectedTokenError.ExpectedTokens).CountIs(5);
            Check.That(unexpectedTokenError.ExpectedTokens.Extracting(x => x.TokenId)).Contains(new[]
            {
                ExpressionToken.FACTORIAL,ExpressionToken.DIVIDE,ExpressionToken.TIMES,ExpressionToken.MINUS,ExpressionToken.PLUS
            });
        }
    }
}