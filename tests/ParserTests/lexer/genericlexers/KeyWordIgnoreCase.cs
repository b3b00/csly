using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer(KeyWordIgnoreCase = true)]
public enum KeyWordIgnoreCase
{
    [Lexeme(GenericToken.KeyWord, "keyword")]
    KEYWORD = 1
}