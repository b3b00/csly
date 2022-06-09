using sly.lexer;

namespace sly.parser.generator
{
    public enum EbnfToken
    {
        [Lexeme("^[A-Za-z][A-Za-z0-9_]*")] IDENTIFIER = 1,
        [Lexeme(":")] COLON = 2,
        [Lexeme("\\*")] ZEROORMORE = 3,
        [Lexeme("\\+")] ONEORMORE = 4,
        [Lexeme("[ \\t]+", true)] WS = 5,
        [Lexeme("^\\?")] OPTION = 6,
        [Lexeme("^\\[d\\]")] DISCARD = 7,

        [Lexeme("^\\(")] LPAREN = 8,

        [Lexeme("^\\)")] RPAREN = 9,

        [Lexeme("[\\n\\r]+", true, true)] EOL = 10
    }
    
    public enum EbnfTokenGeneric
    {
        [Lexeme(GenericToken.Identifier,IdentifierType.AlphaNumericDash)] 
        IDENTIFIER = 1,
        
        [Lexeme(GenericToken.SugarToken,":")]
        COLON = 2,
        
        [Lexeme(GenericToken.SugarToken,"*")]
        ZEROORMORE = 3,
        
        [Lexeme(GenericToken.SugarToken,"+")]
        ONEORMORE = 4,
        
        [Lexeme(GenericToken.SugarToken,"?")]
        OPTION = 6,
        
        [Lexeme(GenericToken.SugarToken,"[d]")]
        DISCARD = 7,
        
        [Lexeme(GenericToken.SugarToken,"(")]
        LPAREN = 8,
        
        [Lexeme(GenericToken.SugarToken,")")]
        RPAREN = 9,
        
        [Lexeme(GenericToken.SugarToken,"|")]
        OR = 10,
        
        [Lexeme(GenericToken.SugarToken,"[")]
        LCROG = 11,
        
        [Lexeme(GenericToken.SugarToken,"]")]
        RCROG = 12,

        [Lexeme(GenericToken.String, "'","\\")]  
        STRING = 13,

    }
}