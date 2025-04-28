using sly.lexer;

namespace ParserExample;

public enum SimpleStackLexer
{
    [Int] INT,
    [Sugar("+")] PLUS
}