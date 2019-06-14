using sly.lexer;

namespace benchCurrent.json
{
    public enum JsonTokenGeneric
    {
        [Lexeme(GenericToken.String)] STRING = 1,
        [Lexeme(GenericToken.Double)] DOUBLE = 2,
        [Lexeme(GenericToken.Int)] INT = 3,

        [Lexeme(GenericToken.KeyWord, "true", "false")]
        BOOLEAN = 4,
        [Lexeme(GenericToken.SugarToken, "{")] ACCG = 5,
        [Lexeme(GenericToken.SugarToken, "}")] ACCD = 6,
        [Lexeme(GenericToken.SugarToken, "[")] CROG = 7,
        [Lexeme(GenericToken.SugarToken, "]")] CROD = 8,
        [Lexeme(GenericToken.SugarToken, ",")] COMMA = 9,
        [Lexeme(GenericToken.SugarToken, ":")] COLON = 10,
        [Lexeme(GenericToken.KeyWord, "null")] NULL = 14
    }
}