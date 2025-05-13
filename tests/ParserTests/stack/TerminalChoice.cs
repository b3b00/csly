using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.stack;

public class TerminalChoice
{
    [Production("root : c")]
    public string Root(string c) => c;
    
    [Production("c : [ A | B ]")]
    public string C(Token<L> ab)  => ab.Value;
}

