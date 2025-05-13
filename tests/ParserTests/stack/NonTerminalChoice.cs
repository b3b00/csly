using sly.lexer;
using sly.parser.generator;

namespace ParserTests.stack;

public class NonTerminalChoice
{
    [Production("root : c")]
    public string Root(string c) => c;
    
    [Production("c : [ a | b ]")]
    public string C(string ab)  => ab;
    
    [Production("a : A")]
    public string A(Token<L> a) => a.Value;
    
    [Production("b: B")]
    public string B(Token<L> b) => b.Value;
}