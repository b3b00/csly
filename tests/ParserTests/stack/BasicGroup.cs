using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.stack;

public class BasicGroup
{
    [Production("root : g")]
    public string Root(string g) => g;
    
    [Production("g : ( a b) ")]
    public string G(Group<L,string> group) => group.Value(0)+" "+group.Value(1);
    
    [Production("a : A")]
    public string A(Token<L> a) => a.Value;
        
    [Production("b: B")]
    public string B(Token<L> b) => b.Value;
}