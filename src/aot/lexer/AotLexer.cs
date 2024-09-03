using sly.lexer;

namespace aot.lexer;

[Lexer(IgnoreWS = true, KeyWordIgnoreCase = true)]
public enum AotLexer
{
    [Lexeme("$-$")]
    PATTERN,
    
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
    [Sugar("²")]
    SQUARE
}