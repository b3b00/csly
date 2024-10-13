using sly.lexer;

namespace ParserTests.Issue487;

public enum Issue487Token
{

    //[Sugar("[ \\t]+")] // the lexeme is marked isSkippable : it will not be sent to the parser and simply discarded.
    //WS = 3,

    [Keyword("CC")]
    [Push("calcmode")]
    [Mode("default")]
    START_C = 102,

    [Keyword("XL")]
    [Push("xlmode")]
    [Mode("default")]
    START_X = 100,           // I think rightmost operand need smaller priority than left operand.  Or it'll be extracted last from the leftovers.  (Moved from 102 to 53 and the right side finally pulled all of the necessary characters)

    [Sugar("@")]
    [Push("dmode")]
    [Mode("default")]
    START_D = 101,

    [Sugar("$")]
    [Push("lvar")]
    [Mode("default")]
    VARSTART = 60,

    //[Lexeme(GenericToken.Identifier, IdentifierType.Custom, "a-zA-Z0-9\\?", "-_0-9A-Za-z/\\.,=\\(){} \\+\\*[]\"")]
    //[Mode("dmode", "xlmode")]
    //OPS = 103,

    //[Sugar("\0")]
    //[Sugar("<<")]
    //[Mode("dmode", "xlmode")]
    //[Pop]
    //CLOSE_M = 104,


    [Lexeme(GenericToken.UpTo, ">>", "??")]
    [Mode("xlmode", "dmode", "calcmode", "lvar")]
    [Pop]
    OPS = 103,

    [Sugar(">>")]
    [Mode("default")]
    INSTALL = 15,

    [Sugar("??")]
    [Mode("default")]
    IFNULLTHEN = 49,

    [Sugar("{")]
    [Mode("default")]
    BLOCK_START = 50,

    [Sugar("}")]
    [Mode("default")]
    BLOCK_END = 48,

    [Sugar("(")]
    [Mode("default")]
    LPAREN = 40,

    [Sugar(")")]
    [Mode("default")]
    RPAREN = 41,

    [Sugar(":")]
    [Mode("convert")]
    [Pop]
    COLON = 43,

    [Sugar("?")]
    [Mode("convert")]
    ISNULLABLE = 105,

    [Keyword("Int")]
    [Keyword("String")]
    [Keyword("Optset")]
    [Keyword("Money")]
    [Keyword("Decimal")]
    [Keyword("EtnRef")]
    [Push("convert")]
    CONVERT = 62,

    [Lexeme(GenericToken.String, "\"", "\\")]
    CONST_BLOCK = 31,


    [AlphaId]
    LOCALVAR = 61,

    EOF = 0

    //[String]
    //STRING = 4
}