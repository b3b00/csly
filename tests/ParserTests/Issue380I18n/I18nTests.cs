using System.Linq;
using expressionparser;
using NFluent;
using simpleExpressionParser;
using sly.parser;
using sly.parser.generator;
using Xunit;
using ExpressionToken = simpleExpressionParser.ExpressionToken;

namespace ParserTests.Issue380I18n;

public class I18nTests
{
    private static Parser<ExpressionToken, double> BuildParser(string i18n)
    {   
        var StartingRule = $"{nameof(SimpleExpressionParser)}_expressions";
        var parserInstance = new SimpleExpressionParser();
        var builder = new ParserBuilder<ExpressionToken, double>(i18n);
        var pBuild = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
        Check.That(pBuild).IsOk();
        return pBuild.Result;
    }
    
    [Fact]
    public static void TestErrorMessage()
    {
        var parser = BuildParser("en");
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
        Check.That(unexpectedTokenError.UnexpectedToken.Position.Line).IsEqualTo(1);
        Check.That(unexpectedTokenError.UnexpectedToken.Position.Column).IsEqualTo(3);
        Check.That(unexpectedTokenError.ExpectedTokens.Extracting(x => x.TokenId)).Contains(new[]
        {
            ExpressionToken.FACTORIAL,ExpressionToken.DIVIDE,ExpressionToken.TIMES,ExpressionToken.MINUS,ExpressionToken.PLUS
        });
        Check.That(error.ErrorMessage).Contains("(line 1, column 3)");
        Check.That(error.ErrorMessage).Contains("opening parenthesis");

    }
}