using sly.lexer;

namespace ParserTests.Issue164
{
    public enum TestOption164Lexer
    {
        [Lexeme(GenericToken.Int)]
        INT = 0,
        
        [Lexeme(GenericToken.Identifier,IdentifierType.Alpha)]
        ID = 1,
        
        [Lexeme(GenericToken.SugarToken,"+")]
        PLUS,
        [Lexeme(GenericToken.SugarToken,"-")]
        MINUS,
        [Lexeme(GenericToken.SugarToken,"*")]
        TIMES,
        [Lexeme(GenericToken.SugarToken,"/")]
        DIVIDE,
        [Lexeme(GenericToken.SugarToken,"(")]
        PAREN
        
    }
}