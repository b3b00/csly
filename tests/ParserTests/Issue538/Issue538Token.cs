using sly.lexer;

namespace ParserTests.Issue538;

public enum Issue538Token
{
    [Sugar("(")]
    OpenParen,
    [Sugar(")")]
    CloseParen,
    [Sugar(";")]
    Semicolon,
    [Sugar(":")]
    Colon,
    
    [Keyword("For")]
    For,
    
    [Keyword("While")]
    While
}