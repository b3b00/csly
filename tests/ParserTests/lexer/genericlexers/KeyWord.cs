using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum KeyWord
{
    [Lexeme(GenericToken.KeyWord, "keyword")]
    KEYWORD = 1
}