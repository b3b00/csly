using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue485;

[ParserRoot("root")]
public class Issue485SelfEscapeParser
{
    [Production("root : PROPERTY[d] COLON[d] STRING")]
    public string Parse(Token<Issue485SelfEscapeLexer> str)
    {
        return str.StringWithoutQuotes;
    }
}