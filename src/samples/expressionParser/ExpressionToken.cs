using sly.i18n;
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
        [LexemeLabel("en","plus sign")]
        [LexemeLabel("fr","plus")]
        [Lexeme("\\+")] PLUS = 5,

        // the - operator
        [LexemeLabel("en","minus sign")]
        [LexemeLabel("fr","moins")]
        [Lexeme("\\-")] MINUS = 6,

        // the * operator
        [LexemeLabel("en","times sign")]
        [LexemeLabel("fr","multiplication")]
        [Lexeme("\\*")] TIMES = 7,

        //  the  / operator
        [LexemeLabel("en","divide sign")]
        [LexemeLabel("fr","division")]
        [Lexeme("\\/")] DIVIDE = 8,

        // a left paranthesis (
        [LexemeLabel("en","opening parenthesis")]
        [LexemeLabel("fr","parenthèse ouvrante")]
        [Lexeme("\\(")] LPAREN = 9,

        // a right paranthesis )
        [LexemeLabel("en","closing parenthesis")]
        [LexemeLabel("fr","parenthèse fermante")]
        [Lexeme("\\)")] RPAREN = 10,

        [LexemeLabel("fr","point d'exclamation")]
        [LexemeLabel("en","exclamation point")]
        [Lexeme("!")] FACTORIAL = 13,


        // a whitespace
        [Lexeme("[ \\t]+", true)] WS = 11,

        [Lexeme("[\\n\\r]+", true, true)] EOL = 12


    }
}
