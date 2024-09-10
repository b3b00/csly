using sly.parser.generator;

namespace GenericLexerWithCallbacks;

[ParserRoot("root")]
public class ParserCallbacks
{
    [Production("root: IF[d] THEN[d] ELSE[d]")]
    public object Root()
    {
        return null;
    }
}