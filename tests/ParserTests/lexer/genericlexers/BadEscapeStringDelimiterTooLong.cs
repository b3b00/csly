using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum BadEscapeStringDelimiterTooLong
    {
        [Lexeme(GenericToken.String, "'", ";:")] toolong
    }
}