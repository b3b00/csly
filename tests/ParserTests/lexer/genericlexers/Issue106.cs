using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum Issue106
{
    [Lexeme(GenericToken.Int)] Integer = 5,

    [Lexeme(GenericToken.Double)] Double = 6,

    [Lexeme(GenericToken.SugarToken, ".")] Period
}