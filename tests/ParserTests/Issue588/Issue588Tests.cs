using Xunit;
using NFluent;
using sly;
using sly.parser;
using sly.parser.generator;
namespace ParserTests.Issue588;

public class Issue588Tests {


    [Fact]
    public void TestIssue588() {
        ParserBuilder<Issue588Lexer, string> builder = new ParserBuilder<Issue588Lexer, string>("en");
        var instance = new Parse();
        var build = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        Check.That(build).IsOk();
        var source = @"IF x == 12
    IF z == 78
        w = 23
        IF a == 46 
            b = 93
        ELSE    
            b=39
    y = 14";
        var parsed = build.Result.Parse(source);
        Check.That(parsed).IsOkParsing();
    }
}