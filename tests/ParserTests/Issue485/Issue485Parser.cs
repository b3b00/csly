using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue485;

[ParserRoot("root")]
public class Issue485Parser
{
    [Production("root : PROPERTY[d] COLON[d] STRING")]
    public string Parse(Token<Issue485Lexer> str)
    {
        return str.StringWithoutQuotes;
    }
}