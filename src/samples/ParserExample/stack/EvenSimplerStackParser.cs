using sly.lexer;
using sly.parser.generator;

namespace ParserExample;

[ParserRoot("root")]
public class EvenSimplerStackParser
{
    [Production("root : expr")]
    public string root(string e) => e;

    [Production("expr : INT INT")]
    public string expr(Token<SimplerStackLexer> i, Token<SimplerStackLexer> j) => i.Value + "," + j.Value;
    
}