using System;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using NFluent;
using ParserTests.Issue164;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.errorAccuracy;

public class ErrorMessageAccuracyTests
{
    private static Parser<ErrorAccuracyIssue381Token, object> BuildParser()
    {
        var StartingRule = $"statement";
        var parserInstance = new ErrorAccuracyIssue381Parser();
        var builder = new ParserBuilder<ErrorAccuracyIssue381Token, object>();
        var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
        Check.That(parser).IsOk();
        return parser.Result;
    }

    [Fact]
    public void TestAccuracy()
    {
        string source = @"
variable = function(glop, ""a"", ""b"", 
            ""i1"", ""1"",
            ""i2"", ""2""
            ""i3"", ""3"",
            ""i4"", ""4"")
";

        var parser = BuildParser();
        var r = parser.Parse(source);
        Check.That(r).Not.IsOkParsing();
        var error = r.Errors.First();
        Check.That(r.Errors).IsSingle();
        var eror = r.Errors.First() as UnexpectedTokenSyntaxError<ErrorAccuracyIssue381Token>;
        Check.That(error).IsNotNull();
        Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(error.Column).IsEqualTo(12);
        Check.That(error.Line).IsEqualTo(4);
        Check.That(error.ErrorMessage).Contains("STRING");
    }
}