using sly.lexer;

namespace ParserTests
{
    public enum RegexLexAndExplicitTokensLexer
    {
        [Lexeme("[0-9]*")]
        INT = 0,
    }
}