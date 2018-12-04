using sly.lexer;

namespace expressionparser
{
    public enum ExpressionToken
    {
        // float number 
        [Lexeme("[0-9]+\\.[0-9]+")] DOUBLE = 1,

        // integer        
        [Lexeme("[0-9]+")] INT = 3,

        [Lexeme("[a-zA-Z]+")] IDENTIFIER = 4,

        // the + operator
        [Lexeme("\\+")] PLUS = 5,

        // the - operator
        [Lexeme("\\-")] MINUS = 6,

        // the * operator
        [Lexeme("\\*")] TIMES = 7,

        //  the  / operator
        [Lexeme("\\/")] DIVIDE = 8,

        // a left paranthesis (
        [Lexeme("\\(")] LPAREN = 9,

        // a right paranthesis )
        [Lexeme("\\)")] RPAREN = 10,

        [Lexeme("!")] FACTORIAL = 13,


        // a whitespace
        [Lexeme("[ \\t]+", true)] WS = 11,

        [Lexeme("[\\n\\r]+", true, true)] EOL = 12,
        
        
    }
}