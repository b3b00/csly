using sly.lexer;

namespace ParserTests.lexer
{
    public enum CharTokensConflicts{
        [Lexeme(GenericToken.Char,"'","\\")]
        [Lexeme(GenericToken.Char,"|","\\")]
        MyChar,

        [Lexeme(GenericToken.Char,"|","\\")]
        OtherChar,

        [Lexeme(GenericToken.String,"'","\\")]
        MyString
    }
}