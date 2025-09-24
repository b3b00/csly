using NFluent;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue485;

public class Issue485Tests
{
    [Theory]
    [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
    [InlineData(ParserType.EBNF_LL_STACK)]
    public void TestIssue485(ParserType parserType)
    {
        var builder = new ParserBuilder<Issue485Lexer, string>();
        var build = builder.BuildParser(new Issue485Parser(), parserType,"root");
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
    
    [Theory]
    [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
    [InlineData(ParserType.EBNF_LL_STACK)]
    public void TestIssue485SelfEscape(ParserType parserType)
    {
        var builder = new ParserBuilder<Issue485SelfEscapeLexer, string>();
        var build = builder.BuildParser(new Issue485SelfEscapeParser(), parserType,"root");
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
    
    [Theory]
    [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
    [InlineData(ParserType.EBNF_LL_STACK)]
    public void TestIssue485WithCallback(ParserType parserType)
    {
        var builder = new ParserBuilder<Issue485WithCallbackLexer, string>();
        var build = builder.BuildParser(new Issue485WithCallbackParser(), parserType,"root");
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