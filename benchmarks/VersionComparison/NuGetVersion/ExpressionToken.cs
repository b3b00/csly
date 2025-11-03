using sly.lexer;

namespace NuGetVersion;

public enum ExpressionToken
{
    [Double]
    NUMBER,
    [Sugar("+")]
    PLUS,
    [Sugar("-")]
    MINUS,
    [Sugar("*")]
    MULTIPLY,
    [Sugar("/")]
    DIVIDE,
    [Sugar("(")]
    LPAREN,
    [Sugar(")")]
    RPAREN,
    EOF
}