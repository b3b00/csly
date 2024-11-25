using sly.lexer;

namespace jsonparser
{
    public enum JsonTokenGeneric
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
    
    public enum JsonTokenGenericStringNotEscaped
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