using sly.lexer;

namespace ParserTests.Issue223_EarlyEos
{
    public enum EarlyEosToken
    {
        [Lexeme(@"OR")]
        OR = 1,
        [Lexeme(@"AND")]
        AND,
        [Lexeme(@"NOT")]
        NOT,
        [Lexeme(@"\+")]
        PLUS,
        [Lexeme(@"\-")]
        MINUS,
        
        [Lexeme(@"\'(\\\'|[^\'])*?\'")]
        [Lexeme(@"\""(\\\""|[^\""])*?\""")]
        QUOTED_VALUE,
        
        [Lexeme(@":")]
        COLON,

        [Lexeme(@"-?[0-9]*\.[0-9]+")]
        DOUBLE,

        [Lexeme(@"-?[0-9]+")]
        INT,
        
        [Lexeme(@"\[")]
        LBRACKET,
        [Lexeme(@"\[")]
        RBRACKET,
        
        [Lexeme(@"\(")]
        LPAREN,
        [Lexeme(@"\)")]
        RPAREN,
        
        [Lexeme(@"\s+")]
        SPACE,

        [Lexeme(@"[\w\-\._@#+&/\=*~]+")]
        VALUE
    }
}