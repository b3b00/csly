using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum Issue114
{
    [Lexeme(GenericToken.SugarToken, "//")]
    First = 1,

    [Lexeme(GenericToken.SugarToken, "/*")]
    Second = 2
}