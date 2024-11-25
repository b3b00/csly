using sly.lexer;

namespace bench.json
{
    public enum JsonTokenGenericNotEscaped
    {
        [String(doEscape:false, channel:0)] STRING = 1,
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
    
    public enum JsonTokenGenericEscaped
    {
        [String(doEscape:true, channel:0)] STRING = 1,
        [Double(channel:0)] DOUBLE = 2,
        [Lexeme(GenericToken.Int,channel:0)] INT = 3,

        [Keyword("true", channel:0)]
        [Keyword("false", channel:0)]
        BOOLEAN = 4,
        [Keyword( "null",channel:0)] NULL = 14,
        
        
        [Sugar("{",channel:0)] ACCG = 5,
        [Sugar( "}",channel:0)] ACCD = 6,
        [Sugar( "[",channel:0)] CROG = 7,
        [Sugar( "]",channel:0)] CROD = 8,
        [Sugar( ",",channel:0)] COMMA = 9,
        [Sugar( ":",channel:0)] COLON = 10,
        
    }
}