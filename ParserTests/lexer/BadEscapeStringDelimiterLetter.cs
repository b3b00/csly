using sly.lexer;

namespace ParserTests.lexer
{
    public enum BadEscapeStringDelimiterLetter
    {
        [Lexeme(GenericToken.String, "'", "a")] toolong
    }
}