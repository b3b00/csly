using sly.lexer;
using sly.parser.generator;

namespace ParserExample;

[ParserRoot("root")]
public class SimpleStackParser
{
    [Production("root : expr")]
    public int root(int e) => e;
    
    [Production("expr : INT PLUS expr")]
    public int expr(Token<SimpleStackLexer> e1, Token<SimpleStackLexer> plus, int e2) => e1.IntValue + e2;
    
    [Production("expr : term")]
    // [Production("expr : INT")]
    //public int expr2(Token<SimpleStackLexer> e) => e.IntValue;
    public int expr2(int e) => e;
    
    [Production("term : INT")]
    public int term(Token<SimpleStackLexer> i) => i.IntValue;
}