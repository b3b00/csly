using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.stack;

public class ManyGroup
{
    [Production("root : g")]
    public string Root(string g) => g;
    
    [Production("g : ( a b)* ")]
    public string G(List<Group<L,string>> groups)
    {
        return string.Join(",",groups.Select(x => x.Value(0)+" "+x.Value(1)));
    }

    [Production("a : A")]
    public string A(Token<L> a) => a.Value;
        
    [Production("b: B")]
    public string B(Token<L> b) => b.Value;
}