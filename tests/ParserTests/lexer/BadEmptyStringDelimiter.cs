using sly.lexer;

namespace ParserTests.lexer
{
    public enum BadEmptyStringDelimiter
    {
        [Lexeme(GenericToken.String, "")] Empty
    }
}