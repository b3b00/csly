using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum BadLetterStringDelimiter
    {
        [Lexeme(GenericToken.String, "a")] Letter
    }
}