using sly.lexer;

namespace ParserExample
{
    
    [Lexer(Indentation = "\t",IndentationAWare = true)]
    public enum IndentedLangLexer
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
        ID = 1,

        [Lexeme(GenericToken.Double)] 
        DOUBLE = 2,

        [Lexeme(GenericToken.Int)] [Lexeme(GenericToken.Int)]
        INT = 3
        
    }
}