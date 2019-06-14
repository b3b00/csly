using sly.lexer;

namespace benchCurrent.json
{
    public enum JsonToken
    {
        [Lexeme("(\\\")([^(\\\")]*)(\\\")")] STRING = 1,
        [Lexeme("[0-9]+\\.[0-9]+")] DOUBLE = 2,
        [Lexeme("[0-9]+")] INT = 3,
        [Lexeme("(true|false)")] BOOLEAN = 4,
        [Lexeme("{")] ACCG = 5,
        [Lexeme("}")] ACCD = 6,
        [Lexeme("\\[")] CROG = 7,
        [Lexeme("\\]")] CROD = 8,
        [Lexeme(",")] COMMA = 9,
        [Lexeme(":")] COLON = 10,
        [Lexeme("[ \\t]+", true)] WS = 12,
        [Lexeme("[\\n\\r]+", true, true)] EOL = 13,
        [Lexeme("(null)")] NULL = 14
    }
}