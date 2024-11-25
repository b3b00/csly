using sly.i18n;
using sly.lexer;

namespace bench.simpleExpressionParser;

public enum GenericExpressionToken
{
    // float number 
    [Double] DOUBLE = 1,

    // integer        
    [Int] INT = 3,

    [AlphaNumId] IDENTIFIER = 4,

    // the + operator
    [LexemeLabel("en", "plus sign")] [LexemeLabel("fr", "plus")] 
    [Sugar("+")]
    PLUS = 5,

    // the - operator
    [LexemeLabel("en", "minus sign")] [LexemeLabel("fr", "moins")] [Sugar("-")]
    MINUS = 6,

    // the * operator
    [LexemeLabel("en", "times sign")] [LexemeLabel("fr", "multiplication")] [Sugar("*")]
    TIMES = 7,

    //  the  / operator
    [LexemeLabel("en", "divide sign")] [LexemeLabel("fr", "division")] [Sugar("/")]
    DIVIDE = 8,

    // a left paranthesis (
    [LexemeLabel("en", "opening parenthesis")] [LexemeLabel("fr", "parenthèse ouvrante")] [Sugar("(")]
    LPAREN = 9,

    // a right paranthesis )
    [LexemeLabel("en", "closing parenthesis")] [LexemeLabel("fr", "parenthèse fermante")] [Sugar(")")]
    RPAREN = 10,

    [LexemeLabel("fr", "point d'exclamation")] [LexemeLabel("en", "exclamation point")] [Sugar("!")]
    FACTORIAL = 13,

}