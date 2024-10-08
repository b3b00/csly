namespace sly.sourceGenerator.generated.ebnfparser;

public enum EbnfRuleToken
{
    None = 0,
    
    //[Lexeme(GenericToken.Identifier,IdentifierType.AlphaNumericDash)] 
    IDENTIFIER = 1,
        
    //[Lexeme(GenericToken.SugarToken,":")]
    COLON = 2,
        
    //[Lexeme(GenericToken.SugarToken,"*")]
    ZEROORMORE = 3,
        
    //[Lexeme(GenericToken.SugarToken,"+")]
    ONEORMORE = 4,
        
    //[Lexeme(GenericToken.SugarToken,"?")]
    OPTION = 6,
        
    //[Lexeme(GenericToken.SugarToken,"[d]")]
    DISCARD = 7,
        
    //[Lexeme(GenericToken.SugarToken,"(")]
    LPAREN = 8,
        
    //[Lexeme(GenericToken.SugarToken,")")]
    RPAREN = 9,
        
    //[Lexeme(GenericToken.SugarToken,"|")]
    OR = 10,
        
    //[Lexeme(GenericToken.SugarToken,"[")]
    LCROG = 11,
        
    //[Lexeme(GenericToken.SugarToken,"]")]
    RCROG = 12,

    //[Lexeme(GenericToken.String, "'","\\")]  
    STRING = 13,
}