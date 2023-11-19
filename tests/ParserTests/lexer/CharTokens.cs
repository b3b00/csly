using sly.lexer;

namespace ParserTests.lexer
{
    public enum CharTokens {
        [Lexeme(GenericToken.Char,"'","\\")]
        MyChar
    }
}