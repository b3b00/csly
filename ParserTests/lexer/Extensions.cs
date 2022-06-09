using sly.lexer;

namespace ParserTests.lexer
{
    public enum Extensions
    {
        [Lexeme(GenericToken.Extension,channel:0)] DATE,

        [Lexeme(GenericToken.Double,channel:0)] DOUBLE
    }
}