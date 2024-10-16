using sly.lexer;
using sly.parser.generator;

namespace ParserTests.lexer.genericlexers;

public class Issue186MixedGenericAndRegexParser
{
    [Production("root : INT")]
    public object root(Token<Issue186MixedGenericAndRegexLexer> integer)
    {
        return null;
    }
}