using System;
using sly.lexer;

namespace SimpleTemplate
{
    
    [Lexer(IgnoreEOL = true)]
    public enum TemplateLexer
    {
 
        #region TEXT
   
        [UpTo("{%", "{=")]
        TEXT,
        
        [Sugar("{%")] [Push("code")] OPEN_CODE,
        
        [Sugar("{=")] [Push("value")] OPEN_VALUE,

    #endregion

    #region value

    [AlphaId]
    [Mode("value")]
    [Mode("code")]
    ID,
    
    [Sugar("=}")]
    [Mode("value")]
    [Pop]
    CLOSE_VALUE,

    #endregion
    
    #region code
    
    [Sugar("%}")]
    [Mode("code")]
    [Pop]
    CLOSE_CODE,
    
    [Keyword("if")]
    [Mode("code")]
    IF,
    
    [Keyword("endif")]
    [Mode("code")]
    ENDIF,
    
    [Keyword("else")]
    [Mode("code")]
    ELSE,
    
    [Keyword("for")] 
    [Mode("code")]
    FOR,

    [Keyword("as")] 
    [Mode("code")]
    AS,
    
    [Keyword("end")] 
    [Mode("code")]
    END,
    
    [Sugar("..")]
    [Mode("code")]
    RANGE,
    
    #region literals
    
    [String()]
    [Mode("code")]
    STRING,
    
    // [Int()]
    // [Mode("code")]
    // INT,
    
    [Int()]
    [Mode("code")]
    INT,
    
    [Lexeme(GenericToken.KeyWord, "TRUE")]
    [Lexeme(GenericToken.KeyWord, "true")]
    [Mode("code")]
    TRUE,

    [Lexeme(GenericToken.KeyWord, "FALSE")]
    [Lexeme(GenericToken.KeyWord, "false")]
    [Mode("code")]
    FALSE,
    
    
    
    #endregion
    
    #region operators 30 -> 49

    [Sugar( ">")]
    [Mode("code")]
    GREATER = 30,

    [Sugar( "<")]
    [Mode("code")]
    LESSER = 31,

    [Sugar( "==")]
    [Mode("code")]
    EQUALS = 32,

    [Sugar( "!=")]
    [Mode("code")]
    DIFFERENT = 33,

    [Sugar( "&")]
    [Mode("code")]
    CONCAT = 34,

    [Sugar( ":=")]
    [Mode("code")]
    ASSIGN = 35,

    [Sugar( "+")]
    [Mode("code")]
    PLUS = 36,

    [Sugar( "-")]
    [Mode("code")]
    MINUS = 37,


    [Sugar( "*")]
    [Mode("code")]
    TIMES = 38,

    [Sugar( "/")]
    [Mode("code")]
    DIVIDE = 39,
    
    
    #endregion
    
    #region sugar 100 -> 150
    
    [Sugar("(")]
    [Mode("code")]
    OPEN_PAREN,
    
    [Sugar(")")]
    [Mode("code")]
    CLOSE_PAREN,
    
    [Lexeme(GenericToken.KeyWord, "NOT")] [Lexeme(GenericToken.KeyWord, "not")]
    [Mode("code")]
    NOT,

    [Lexeme(GenericToken.KeyWord, "AND")] [Lexeme(GenericToken.KeyWord, "and")]
    [Mode("code")]
    AND,

    [Lexeme(GenericToken.KeyWord, "OR")] [Lexeme(GenericToken.KeyWord, "or")]
    [Mode("code")]
    OR,
    
    #endregion
    
    #endregion
        
    }
}