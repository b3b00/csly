using sly.lexer;
using sly.parser.generator;

namespace ParserExample;

[ParserRoot("root")]
public class SimplerStackParser
{
    [Production("root : expr")]
    public string root(string e) => e;

    [Production("expr : INT expr")]
    public string expr(Token<SimplerStackLexer> i, string e) => i.Value + "," + e;
    
    [Production("expr : INT")]
    public string expr2(Token<SimplerStackLexer> i) => i.Value;
    
}