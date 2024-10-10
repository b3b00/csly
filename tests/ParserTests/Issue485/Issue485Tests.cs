using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue485;

public class Issue485Tests
{
    [Fact]
    public void TestIssue485()
    {
        var builder = new ParserBuilder<Issue485Lexer, string>();
        var build = builder.BuildParser(new Issue485Parser(), ParserType.EBNF_LL_RECURSIVE_DESCENT,"root");
        Check.That(build).IsOk();
        var parser = build.Result;
        Check.That(parser).IsNotNull();
        var parsed = parser.Parse("Property: \"Hello \\\"There\\\"\\nSecond line\"");
        Check.That(parsed).IsOkParsing();
        var result = parsed.Result;
        Check.That(result).IsNotNull();
        Check.That(result).IsNotEmpty();
        Check.That(result).Equals("Hello \"There\"\\nSecond line");
    }
    [Fact]
    public void TestIssue485SelfEscape()
    {
        var builder = new ParserBuilder<Issue485SelfEscapeLexer, string>();
        var build = builder.BuildParser(new Issue485SelfEscapeParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT,"root");
        Check.That(build).IsOk();
        var parser = build.Result;
        Check.That(parser).IsNotNull();
        var parsed = parser.Parse("Property: \"Hello \"\"There\"\"\\nSecond line\"");
        Check.That(parsed).IsOkParsing();
        var result = parsed.Result;
        Check.That(result).IsNotNull();
        Check.That(result).IsNotEmpty();
        Check.That(result).Equals("Hello \"There\"\\nSecond line");
    }
    
    [Fact]
    public void TestIssue485WithCallback()
    {
        var builder = new ParserBuilder<Issue485WithCallbackLexer, string>();
        var build = builder.BuildParser(new Issue485WithCallbackParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT,"root");
        Check.That(build).IsOk();
        var parser = build.Result;
        Check.That(parser).IsNotNull();
        var parsed = parser.Parse("Property: \"Hello \\\"There\\\"\\nSecond line\"");
        Check.That(parsed).IsOkParsing();
        var result = parsed.Result;
        Check.That(result).IsNotNull();
        Check.That(result).IsNotEmpty();
        Check.That(result).Equals("Hello \"There\"\nSecond line");
    }
}