using sly.lexer;

namespace ParserTests
{
    public enum RegexLexAndImplicitTokensLexer
    {
        [Lexeme("[0-9]*")]
        INT = 0,
    }
}