using sly.i18n;
using sly.lexer;

namespace ParserTests.Issue239
{
    public enum Issue239Lexer
    {
        [AlphaNumDashId]
        [LexemeLabel("en","identifier")]
        ID,
        
        [LexemeLabel("en","int keyword")]
        [Keyword("int")]
        INT,
        
        [LexemeLabel("en","int literal")]
        [Int]
        INT_LITERAL,
        [Sugar("=")]
        [LexemeLabel("en","equal")]
        
        ASSIGN,
        [Sugar(";")]
        [LexemeLabel("en","semicolon")]
        SEMI
        
        
    }
}