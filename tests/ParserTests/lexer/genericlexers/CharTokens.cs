using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum CharTokens {
        [Lexeme(GenericToken.Char,"'","\\")]
        [Character("|","\\")]
        MyChar
    }
}