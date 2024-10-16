using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum BadEmptyStringDelimiter
    {
        [Lexeme(GenericToken.String, "")] Empty
    }
}