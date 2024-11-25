using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum Issue210Token
    {
        EOF = 0,

        [Lexeme(GenericToken.Extension)] SPECIAL,
        [Lexeme(GenericToken.KeyWord, "x")] X,
        [Lexeme(GenericToken.SugarToken, "?")] QMARK
    }
}