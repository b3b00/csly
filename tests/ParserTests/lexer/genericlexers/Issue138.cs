using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum Issue138
{
    [Lexeme(GenericToken.SugarToken, "..")]
    A = 1,
    [Lexeme(GenericToken.SugarToken, "-")] B,

    [Lexeme(GenericToken.SugarToken, "---")]
    C
}