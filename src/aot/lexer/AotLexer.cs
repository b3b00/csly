using sly.lexer;

namespace aot.lexer;

public enum AotLexer
{
    [Double]
    DOUBLE,
    [AlphaId]
    IDENTIFIER,
    [Sugar("+")]
    PLUS,
    [Sugar("++")]
    INCREMENT,
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
    [Sugar("!")]
    FACTORIAL,
}