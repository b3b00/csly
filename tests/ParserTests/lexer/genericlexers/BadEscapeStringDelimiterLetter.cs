using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum BadEscapeStringDelimiterLetter
    {
        [Lexeme(GenericToken.String, "'", "a")] toolong
    }
}