using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.stack;

public class TerminalChoice
{
    [Production("root : c")]
    public string Root(string c) => c;
    
    [Production("c : [ A | B ]")]
    public string C(Token<L> ab)  => ab.Value;
}

public class ManyTerminalChoice
{
    [Production("root : c")]
    public string Root(string c) => c;
    
    [Production("c : [ A | B ]*")]
    public string C(List<Token<L>> abs)
    {
        return string.Join(",", abs.Select(x => x.Value));
    }
}