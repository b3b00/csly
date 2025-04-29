using sly.lexer;

namespace ParserExample;

public enum SimpleStackLexer
{
    EOS,
    [Int] INT,
    [Sugar("+")] PLUS
}