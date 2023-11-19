using System.Collections.Generic;
using System.Linq;
using NFluent;
using ParserTests.errorAccuracyIssue381;
using sly.parser;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using Xunit;

namespace ParserTests.Issue381ErrorAccuracy;

public class ErrorMessageAccuracyIssue381Tests
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
        variable = function(someVariable, ""string1"", ""string2"", 
            ""string3"", ""value1"",
            ""string4"", ""value2""
            ""string5"", ""value3"",
            ""string6"", ""value4""
";

        var parser = BuildParser();
        var r = parser.Parse(source,"statements");
        Check.That(r).Not.IsOkParsing();
        var error = r.Errors.First();
        Check.That(r.Errors).IsSingle();
        var unexpected = r.Errors.First() as UnexpectedTokenSyntaxError<ErrorAccuracyIssue381Token>;
        Check.That(unexpected).IsNotNull();
        Check.That(unexpected.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(unexpected.Column).IsEqualTo(12);
        Check.That(unexpected.Line).IsEqualTo(4);
        Check.That(unexpected.UnexpectedToken.StringWithoutQuotes).IsEqualTo("string5");
        Check.That(unexpected.UnexpectedToken.TokenID).IsEqualTo(ErrorAccuracyIssue381Token.String);
        Check.That(unexpected.ExpectedTokens.Extracting(x => x.TokenId))
            .Contains(new List<ErrorAccuracyIssue381Token>()
                { ErrorAccuracyIssue381Token.Comma, ErrorAccuracyIssue381Token.Rparen });
        
    }
    
    [Fact]
    public void TestAccuracy2()
    {
        string source = @"
        [
,""1""
,""2""
,3
4
,V
,six
]            
";

        var parser = BuildParser();
        var r = parser.Parse(source,"statements");
        Check.That(r).Not.IsOkParsing();
        var error = r.Errors.First();
        Check.That(r.Errors).IsSingle();
        var unexpected = r.Errors.First() as UnexpectedTokenSyntaxError<ErrorAccuracyIssue381Token>;
        Check.That(unexpected).IsNotNull();
        Check.That(unexpected.ErrorType).IsEqualTo(ErrorType.UnexpectedToken);
        Check.That(unexpected.Column).IsEqualTo(0);
        Check.That(unexpected.Line).IsEqualTo(5);
        Check.That(unexpected.UnexpectedToken.StringWithoutQuotes).IsEqualTo("4");
        Check.That(unexpected.UnexpectedToken.TokenID).IsEqualTo(ErrorAccuracyIssue381Token.Int);
        Check.That(unexpected.ExpectedTokens.Extracting(x => x.TokenId))
            .Contains(new List<ErrorAccuracyIssue381Token>()
                { ErrorAccuracyIssue381Token.Comma, ErrorAccuracyIssue381Token.Rbrack });
        
    }
}