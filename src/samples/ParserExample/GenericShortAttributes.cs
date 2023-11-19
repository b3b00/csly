using sly.lexer;

namespace ParserExample
{
    public enum GenericShortAttributes
    {
        [Double] DOUBLE = 1,

        // integer        
        [Int] INT = 3,

        [AlphaId] IDENTIFIER = 4,

        // the + operator
        [Sugar("+")] PLUS = 5,

        // the ++ operator
        [Sugar("++")]
        INCREMENT = 6,

        // the - operator
        [Sugar("-")] MINUS = 7,

        // the * operator
        [Sugar("*")] TIMES = 8,

        //  the  / operator
        [Sugar("/")] DIVIDE = 9,

        // a left paranthesis (
        [Sugar("(")] LPAREN = 10,

        // a right paranthesis )
        [Sugar(")")] RPAREN = 11,
        
        [String("'","\\")]
        STRING = 12,
        
        [Keyword("hello")]
        HELLO = 13
    }
}