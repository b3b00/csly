using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue443;

public class Test443
{
    [Fact]
    
    public void Issue443Test()
    {
        var test443Parser = new Test443Parser();
        var builder = new ParserBuilder<Test443Lexer, string>();

        var parser = builder.BuildParser(test443Parser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");

        Check.That(parser).IsOk();
        var r = parser.Result.Parse("@@");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo("@@");
        r = parser.Result.Parse("@$$$@");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo("@$$$@");
        r = parser.Result.Parse("@$@");
        Check.That(r).IsOkParsing();
        Check.That(r.Result).IsEqualTo("@$@");
        
    }
}