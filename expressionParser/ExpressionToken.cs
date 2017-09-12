namespace expressionparser
{
    public enum ExpressionToken
    {
        INT = 2, // integer
        DOUBLE = 3, // float number 
        PLUS = 4, // the + operator
        MINUS = 5, // the - operator
        TIMES = 6, // the * operator
        DIVIDE = 7, //  the  / operator
        LPAREN = 8, // a left paranthesis (
        RPAREN = 9,// a right paranthesis )
        WS = 12, // a whitespace
        EOL = 14
    }
}
