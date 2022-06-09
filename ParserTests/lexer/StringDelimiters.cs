using sly.lexer;

namespace ParserTests.lexer
{
    public enum StringDelimiters {
        [Lexeme(GenericToken.String,"'","'")]
        MyString
    }
}