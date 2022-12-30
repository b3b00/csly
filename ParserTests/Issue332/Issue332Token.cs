using sly.lexer;

namespace ParserTests.Issue332;

[Lexer(IndentationAWare = true)]
public enum Issue332Token
{
    [Lexeme(GenericToken.KeyWord, "IF")] [Lexeme(GenericToken.KeyWord, "if")]
    IF = 1,

    [Lexeme(GenericToken.KeyWord, "ELIF")] [Lexeme(GenericToken.KeyWord, "elif")]
    ELIF = 2,

    [Lexeme(GenericToken.KeyWord, "ELSE")] [Lexeme(GenericToken.KeyWord, "else")]
    ELSE = 3,

    [Lexeme(GenericToken.KeyWord, "WHILE")] [Lexeme(GenericToken.KeyWord, "while")]
    WHILE = 4,

    [Lexeme(GenericToken.KeyWord, "FOR")] [Lexeme(GenericToken.KeyWord, "for")]
    FOR = 5,
    
    [Lexeme(GenericToken.KeyWord, "TRUE")] [Lexeme(GenericToken.KeyWord, "true")]
    TRUE = 6,

    [Lexeme(GenericToken.KeyWord, "FALSE")] [Lexeme(GenericToken.KeyWord, "false")]
    FALSE = 7,

    [Lexeme(GenericToken.KeyWord, "NOT")] [Lexeme(GenericToken.KeyWord, "not")]
    NOT = 8,

    [Lexeme(GenericToken.KeyWord, "AND")] [Lexeme(GenericToken.KeyWord, "and")]
    AND = 9,
    
    [Lexeme(GenericToken.KeyWord, "OR")] [Lexeme(GenericToken.KeyWord, "or")]
    OR = 10,
    
    [Lexeme(GenericToken.KeyWord,"XOR")][Lexeme(GenericToken.KeyWord,"xor")]
    XOR = 11,
    
    [Sugar("//")]
    ANNOTATION = 12,
    //[Lexeme(GenericToken.KeyWord, "CLASS")] [Lexeme(GenericToken.KeyWord, "print")]
    //PRINT = 12,
    
    [Sugar("\n")]
    HUANHANG = 13,
    
    [Lexeme(GenericToken.KeyWord,"CLASS")][Lexeme(GenericToken.KeyWord,"class")]
    CLASS = 14,
    
    [Lexeme(GenericToken.KeyWord,"FUNC")][Lexeme(GenericToken.KeyWord,"func")]
    FUNC = 15,

    #region literals 20 -> 29
    [Lexeme(GenericToken.Identifier)] IDENTFIER = 20,
    [Lexeme(GenericToken.String)] STRING = 21,
    [Lexeme(GenericToken.Int)] INT = 22,
    [Lexeme(GenericToken.Double)] DOUBLE = 23,
    [Lexeme(GenericToken.Char)] CHAR = 24,
    #endregion

    #region operators 30 -> 49
    
    [Sugar("<-")] SET = 40,

    [Sugar("->")] DIS_SET = 41,
    
    [Sugar( ">")] GREATER = 30,

    [Sugar( "<")] LESSER = 31,

    [Sugar( "==")] EQUALS = 32,

    [Sugar( "!=")] DIFFERENT = 33,

    [Sugar( ".")] CONCAT = 34,

    [Sugar( "-*")] DIRECT = 35,
    
    [Sugar("*-")] DIS_DIRECT = 43,

    [Sugar( "+")] PLUS = 36,

    [Sugar( "-")] MINUS = 37,
    
    [Sugar( "*")] TIMES = 38,

    [Sugar( "/")] DIVIDE = 39,

    #endregion

    #region sugar 50 ->

    [Sugar( "(")] LPAREN = 50,

    [Sugar( ")")] RPAREN = 51,

    EOF = 0

    #endregion
}