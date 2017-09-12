namespace jsonparser
{
    public enum JsonToken
    {
        STRING = 1,
        INT = 2,
        DOUBLE = 3,
        BOOLEAN = 4,
        ACCG = 5,
        ACCD = 6,
        CROG = 7,
        CROD = 8,
        COMMA = 9,
        COLON = 10,
        SEMICOLON = 11,
        WS = 12,
        EOL = 13,
        NULL = 14,
        QUOTE = 99
    }
}
