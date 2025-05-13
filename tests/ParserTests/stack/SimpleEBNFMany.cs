using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.stack;

public class SimpleEBNFMany
{
    [Production("root : astar")]
    public string Root(string astar)
    {
        return astar;
    }
    
    [Production("rootplus : aplus")]
    public string RootPlus(string aplus)
    {
        return aplus;
    }

    [Production("astar : A*")]
    public string Astar(List<Token<L>> all)
    {
        return string.Join(",",all.Select(x => x.Value));
    }
    
    [Production("aplus : A+")]
    public string Aplus(List<Token<L>> all)
    {
        return string.Join(",",all.Select(x => x.Value));
    }
}