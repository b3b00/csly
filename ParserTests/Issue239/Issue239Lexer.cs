using sly.lexer;

namespace ParserTests.Issue239
{
    public enum Issue239Lexer
    {
        [AlphaNumDashId]
        ID,
        [Keyword("int")]
        INT,
        [Int]
        INT_LITERAL,
        [Sugar("=")]
        ASSIGN,
        [Sugar(";")]
        SEMI
        
        
    }
}