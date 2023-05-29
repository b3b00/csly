using sly.lexer;

namespace ParserTests.issue364;

public enum Issue364Token
{
    // float number 
    [Lexeme(GenericToken.Double)] 
    DOUBLE = 1,

    // integer        
    [Lexeme(GenericToken.Int)] 
    INT = 3,

    [Lexeme(GenericToken.Identifier)] 
    IDENTIFIER = 4,

    // the + operator
    [Lexeme(GenericToken.SugarToken, "+")] 
    PLUS = 5,

    // the - operator
    [Lexeme(GenericToken.SugarToken, "-")] 
    MINUS = 6,

    // a whitespace
    //[Lexeme("[ \\t]+", true)] WS = 11,

    //[Lexeme("[\\n\\r]+", true, true)] EOL = 12,
}