using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue485;

[ParserRoot("root")]
public class Issue485WithCallbackParser
{
    [Production("root : PROPERTY[d] COLON[d] STRING")]
    public string Parse(Token<Issue485WithCallbackLexer> str)
    {
        return str.StringWithoutQuotes;
    }
}