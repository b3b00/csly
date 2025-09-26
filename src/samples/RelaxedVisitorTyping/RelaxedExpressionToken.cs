using sly.lexer;

namespace RelaxedVisitorTyping;

internal enum RelaxedExpressionToken
{
    [Lexeme("\\(")]
    LParen = 1,

    [Lexeme("\\)")]
    RParen = 2,

    [Lexeme("[ \\t]+", isSkippable: true)]
    WhiteSpace = 3,

    [Lexeme("[a-zA-Z\\.]+")]
    Property = 4,

    [Lexeme(@"""[^""]+""")]
    String = 5,

    [Lexeme("[0-9]+[\\.,][0-9]+")]
    Number = 6,

    [Lexeme("[0-9]+")]
    Int = 7,

    [Lexeme("-eq(ual)?")]
    Op_Equal = 101,

    [Lexeme("-(lt|lowerthan)")]
    Op_LowerThan = 102,
}