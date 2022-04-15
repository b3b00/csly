using sly.lexer;

namespace simpleExpressionParser
{
    public enum SimpleExpressionToken
    {
        // float number 
        [Lexeme(GenericToken.Double)] DOUBLE = 1,

        // integer        
        [Lexeme(GenericToken.Int)] INT = 3,

        [Lexeme(GenericToken.Identifier)] IDENTIFIER = 4,

        // the + operator
        [Lexeme(GenericToken.SugarToken, "+")] PLUS = 5,

        // the ++ operator
        [Lexeme(GenericToken.SugarToken, "++")]
        INCREMENT = 6,

        // the - operator
        [Lexeme(GenericToken.SugarToken, "-")] MINUS = 7,

        // the * operator
        [Lexeme(GenericToken.SugarToken, "*")] TIMES = 8,

        //  the  / operator
        [Lexeme(GenericToken.SugarToken, "/")] DIVIDE = 9,

        // a left paranthesis (
        [Lexeme(GenericToken.SugarToken, "(")] LPAREN = 10,

        // a right paranthesis )
        [Lexeme(GenericToken.SugarToken, ")")] RPAREN = 11

        // a variable
        
    }
}