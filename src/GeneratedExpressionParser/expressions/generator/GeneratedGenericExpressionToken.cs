
using csly.GeneratedExpressionParser.generatedgenericsimpleexpressionparser.models;
namespace generatedExpressions;

public enum GeneratedGenericExpressionToken
{
    // float number 
    [Double] DOUBLE = 1,

    // integer        
    [Int] INT = 3,

    [AlphaNumId] IDENTIFIER = 4,

    // the + operator
    [Sugar("+")]
    PLUS = 5,

    // the - operator
    [Sugar("-")]
    MINUS = 6,

    // the * operator
    [Sugar("*")]
    TIMES = 7,

    //  the  / operator
    [Sugar("/")]
    DIVIDE = 8,

    // a left paranthesis (
    
    [Sugar("(")]
    LPAREN = 9,

    // a right paranthesis )
    [Sugar(")")]
    RPAREN = 10,

    [Sugar("!")]
    FACTORIAL = 13,

}