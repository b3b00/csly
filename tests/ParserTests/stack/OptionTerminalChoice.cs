using sly.lexer;
using sly.parser.generator;

namespace ParserTests.stack;

public class OptionTerminalChoice
{
    [Production("root : c")]
    public string Root(string c) => c;
    
    [Production("c : [ A | B ]?")]
    public string C(Token<L> ab)
    {
        if (ab.IsEmpty)
        {
            return "nothing";
        }

        return ab.Value;
    }
}