using sly.lexer;

namespace ParserTests.Issue263
{
    public enum Issue263Token
    {
        [Lexeme("\\(")]
        LPARA,

        [Lexeme("\\)")]
        RPARA,

        [Lexeme("a")]
        IDENTIFIER,

        [Lexeme("\\[")]
        LBRAC,

        [Lexeme("\\]")]
        RBRAC
    }
}
