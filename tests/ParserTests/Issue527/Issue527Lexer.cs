using sly.lexer;

namespace ParserTests.Issue527;

public enum Issue527Lexer
{
    [Keyword("a")]
    A = 1,
    
    [Keyword("b")]
    B = 2,
    
}