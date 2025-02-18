using sly.lexer;

namespace ParserTests.Issue219;

public enum Issue219Lexer
{
    [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
    ID = 1,

    [Lexeme(GenericToken.Int)] 
    INT = 2,

    [Lexeme(GenericToken.SugarToken, "=")] 
    EQ = 3 
}