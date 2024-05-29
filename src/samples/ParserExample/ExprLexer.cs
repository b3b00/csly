using sly.lexer;

namespace ParserExample;

public enum ExprLexer
{
    [Double]
    NUMBER,
    [Sugar("+")]
    PLUS,
    [Sugar("-")]
    MINUS,
    [Sugar("*")]
    TIMES,
    [Sugar("/")]
    DIVIDE,
    [Sugar("(")]
    LPAREN,
    [Sugar(")")]
    RPAREN,
}