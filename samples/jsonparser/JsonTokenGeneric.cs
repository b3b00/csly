using sly.lexer;

namespace jsonparser
{
    public enum JsonTokenGeneric
    {
        [Lexeme(GenericToken.String,channel:0)] STRING = 1,
        [Lexeme(GenericToken.Double,channel:0)] DOUBLE = 2,
        [Lexeme(GenericToken.Int,channel:0)] INT = 3,

        [Lexeme(GenericToken.KeyWord,channel:0, "true", "false")]
        BOOLEAN = 4,
        [Lexeme(GenericToken.SugarToken,channel:0, "{")] ACCG = 5,
        [Lexeme(GenericToken.SugarToken,channel:0, "}")] ACCD = 6,
        [Lexeme(GenericToken.SugarToken,channel:0, "[")] CROG = 7,
        [Lexeme(GenericToken.SugarToken,channel:0, "]")] CROD = 8,
        [Lexeme(GenericToken.SugarToken,channel:0, ",")] COMMA = 9,
        [Lexeme(GenericToken.SugarToken,channel:0, ":")] COLON = 10,
        [Lexeme(GenericToken.KeyWord,channel:0, "null")] NULL = 14
    }
}