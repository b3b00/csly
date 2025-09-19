using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue574;

public class Issue574Tests
{
    
    
    
    [Fact]
    public void TestIssue574()
    {
        ParserBuilder<TokenIssue574,object> builder = new ParserBuilder<TokenIssue574,object>();
        var build = builder.BuildParser(new ParserIssue574(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        Check.That(build).IsOk();
        string source = @"type foobar";
        var parsed =  build.Result.Parse(source);
        Check.That(parsed).IsOkParsing(checkIfResultisNull:false);
    }
}