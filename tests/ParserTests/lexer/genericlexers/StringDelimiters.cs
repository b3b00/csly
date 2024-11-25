using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum StringDelimiters {
        [Lexeme(GenericToken.String,"'","'")]
        MyString
    }
}