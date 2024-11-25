using NFluent;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue495;

public class Issue495Tests
{
    public Parser<Issue495Token,string> _parser { get; set; }

    public Parser<Issue495Token, string> GetParser()
    {
        if (_parser == null)
        {
            ParserBuilder<Issue495Token, string> builder = new ParserBuilder<Issue495Token, string>("en");
            var build = builder.BuildParser(new Issue495Parser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            Check.That(build).IsOk();
            _parser = build.Result;
        }

        return _parser;
    }

    [Fact]
    public void TestIssue495()
    {
        var parser = GetParser();
        Check.That(parser).IsNotNull();
        Check.That(parser.Lexer).IsNotNull();
        Check.That(parser.Lexer).IsInstanceOf<GenericLexer<Issue495Token>>();
        var lexer = parser.Lexer as GenericLexer<Issue495Token>;
        string source = "test = \"3 3\";";
        var tokenized = lexer.Tokenize(source);
        Check.That(tokenized).IsOkLexing();
        var tokens = tokenized.Tokens.MainTokens();
        Check.That(tokens).CountIs(7);
        var stringValue = tokens[3];
        Check.That(stringValue).IsNotNull();
        Check.That(stringValue.TokenID).IsEqualTo(Issue495Token.StringValue);
                

        var parsed = parser.Parse(source);
        Check.That(parsed).IsOkParsing();
        Check.That(parsed.Result).IsEqualTo("test=3 3");
    } 
    
}