using sly.lexer;

namespace ParserTests.Issue328;

public enum Issue328Token
{
    [Keyword("XL")]
    [Push("xlmode")]
    START_X = 100,           // I think rightmost operand need smaller priority than left operand.  Or it'll be extracted last from the leftovers.  (Moved from 102 to 53 and the right side finally pulled all of the necessary characters)

    [Sugar("@")]
    [Push("dmode")]
    START_D = 101,
 
    [Lexeme(GenericToken.UpTo, ">>", "\0")]
    [Mode("dmode", "xlmode")]
    [Pop]
    OPS = 103,

    [Sugar(">>")]
    INSTALL = 15,

    [Sugar("(")]
    LPAREN = 40,

    [Sugar(")")]
    RPAREN = 41,

    [Keyword("Int")]
    [Keyword("String")]
    [Keyword("Optset")]
    [Keyword("Money")]
    [Keyword("Decimal")]
    [Keyword("EtnRef")]
    CONVERT = 104,

    [Sugar(":")]
    COLON = 105,
}