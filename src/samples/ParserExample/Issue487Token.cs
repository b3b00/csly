using sly.lexer;

namespace ParserExample;

public enum Issue487Token
{
    [Lexeme(GenericToken.UpTo, ">>", "??")]
    [Mode("xlmode", "dmode", "calcmode", "lvar")]
    [Pop]
    OPS = 103,
    
#region default
    
    [Sugar(">>")]
    [Mode("default")]
    INSTALL = 15,
    
    // PUSHERS
    
    [Keyword("CC")]
    [Push("calcmode")]
    [Mode("default")]
    START_C = 102,

    [Keyword("XL")]
    [Push("xlmode")]
    [Mode("default")]
    START_X = 100,           // I think rightmost operand need smaller priority than left operand.  Or it'll be extracted last from the leftovers.  (Moved from 102 to 53 and the right side finally pulled all of the necessary characters)

    [Sugar("@")]
    [Push("dmode")]
    [Mode("default")]
    START_D = 101,

    [Sugar("$")]
    [Push("lvar")]
    [Mode("default")]
    VARSTART = 60,
    
    [Keyword("Int")]
    [Keyword("String")]
    [Keyword("Optset")]
    [Keyword("Money")]
    [Keyword("Decimal")]
    [Keyword("EtnRef")]
    [Keyword("toconvert")]
    [Push("convert")]
    [Mode("default")]
    CONVERT = 62,
    
    // END PUSHERS
    
    [Sugar("??")]
    [Mode("default")]
    IFNULLTHEN = 49,

    [Sugar("{")]
    [Mode("default")]
    BLOCK_START = 50,

    [Sugar("}")]
    [Mode("default")]
    BLOCK_END = 48,

    [Sugar("(")]
    [Mode("default")]
    LPAREN = 40,

    [Sugar(")")]
    [Mode("default")]
    RPAREN = 41,
    

    
    
    

    [String("\"", "\\")]
    CONST_BLOCK = 31,
    
    [AlphaId]
    LOCALVAR = 61,

    EOF = 0,

#endregion
    
#region dmode

    
#endregion    



#region convert



    [Sugar(":")]
    [Mode("convert")]
    [Pop]
    COLON = 43,
    
    

    [Sugar("?")]
    [Mode("convert")]
    ISNULLABLE = 105,
    
    
#endregion
 


    


}