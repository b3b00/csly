using sly.lexer;
using sly.parser.generator;

namespace ParserExample;

[ParserRoot("root")]
public class SimpleStackParser
{
    [Production("root : expr")]
    public int root(int e) => e;
    
    [Production("expr : term PLUS expr")]
    public int expr(int e1, Token<SimpleStackLexer> plus, int e2) => e1 + e2;
    
    [Production("expr : term")]
    public int expr2(int e) => e;
    
    [Production("term : INT")]
    public int term(Token<SimpleStackLexer> i) => i.IntValue;
}