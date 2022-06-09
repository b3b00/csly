using sly.lexer;

namespace ParserTests.lexer
{
    public enum BadLetterStringDelimiter
    {
        [Lexeme(GenericToken.String, "a")] Letter
    }
}