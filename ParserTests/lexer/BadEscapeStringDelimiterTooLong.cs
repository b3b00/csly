using sly.lexer;

namespace ParserTests.lexer
{
    public enum BadEscapeStringDelimiterTooLong
    {
        [Lexeme(GenericToken.String, "'", ";:")] toolong
    }
}