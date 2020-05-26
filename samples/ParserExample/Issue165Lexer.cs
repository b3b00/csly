using sly.lexer;

namespace ParserExample
{
    public enum Issue165Lexer
    {
        [Lexeme(GenericToken.SugarToken,"?")]
        QUESTIONMARK = 1,
        
        [Lexeme(GenericToken.SugarToken,"+")]
        PLUS = 2,
        
        [Lexeme(GenericToken.SugarToken,"-")]
        MINUS = 3,
        
        [Lexeme(GenericToken.SugarToken,"*")]
        TIMES = 4,
        
        [Lexeme(GenericToken.SugarToken,"/")]
        DIVIDE = 5,
        
        [Lexeme(GenericToken.SugarToken,"(")]
        LPAR = 6,
        
        [Lexeme(GenericToken.SugarToken,")")]
        RPAR = 6,
        
        [Lexeme(GenericToken.SugarToken,"[")]
        LBR = 7,
        
        [Lexeme(GenericToken.SugarToken,"]")]
        RBR = 8,
        
        [Lexeme(GenericToken.SugarToken,":")]
        COLON = 7,
        
        [Lexeme(GenericToken.SugarToken,"=")]
        EQ = 8,
        
        
        [Lexeme(GenericToken.Identifier)] ID = 20,

        [Lexeme(GenericToken.String)] STRING = 21,

        [Lexeme(GenericToken.Int)] INT = 22,
        
        
        
    }
}