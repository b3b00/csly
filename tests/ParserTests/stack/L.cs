using sly.lexer;

namespace ParserTests.stack;

public enum L
{
    [Keyword("A")] A = 1,
    [Keyword("B")] B = 2,
    [Sugar("+")] PLUS = 3,
    [Sugar("-")] MINUS = 4
}