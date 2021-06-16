using sly.lexer;

namespace csly.whileLang.parser
{
    public enum WhileTokenGeneric
    {
        #region keywords 0 -> 19

        [Lexeme(GenericToken.KeyWord, "IF")] [Lexeme(GenericToken.KeyWord, "if")]
        IF = 1,

        [Lexeme(GenericToken.KeyWord, "THEN")] [Lexeme(GenericToken.KeyWord, "then")]
        THEN = 2,

        [Lexeme(GenericToken.KeyWord, "ELSE")] [Lexeme(GenericToken.KeyWord, "else")]
        ELSE = 3,

        [Lexeme(GenericToken.KeyWord, "WHILE")] [Lexeme(GenericToken.KeyWord, "while")]
        WHILE = 4,

        [Lexeme(GenericToken.KeyWord, "DO")] [Lexeme(GenericToken.KeyWord, "do")]
        DO = 5,

        [Lexeme(GenericToken.KeyWord, "SKIP")] [Lexeme(GenericToken.KeyWord, "skip")]
        SKIP = 6,

        [Lexeme(GenericToken.KeyWord, "TRUE")] [Lexeme(GenericToken.KeyWord, "true")]
        TRUE = 7,

        [Lexeme(GenericToken.KeyWord, "FALSE")] [Lexeme(GenericToken.KeyWord, "false")]
        FALSE = 8,

        [Lexeme(GenericToken.KeyWord, "NOT")] [Lexeme(GenericToken.KeyWord, "not")]
        NOT = 9,

        [Lexeme(GenericToken.KeyWord, "AND")] [Lexeme(GenericToken.KeyWord, "and")]
        AND = 10,

        [Lexeme(GenericToken.KeyWord, "OR")] [Lexeme(GenericToken.KeyWord, "or")]
        OR = 11,

        [Lexeme(GenericToken.KeyWord, "PRINT")] [Lexeme(GenericToken.KeyWord, "print")]
        PRINT = 12,

        #endregion

        #region literals 20 -> 29

        [Lexeme(GenericToken.Identifier)] IDENTIFIER = 20,

        [Lexeme(GenericToken.String)] STRING = 21,

        [Lexeme(GenericToken.Int)] INT = 22,

        #endregion

        #region operators 30 -> 49

        [Sugar( ">")] GREATER = 30,

        [Sugar( "<")] LESSER = 31,

        [Sugar( "==")]
        EQUALS = 32,

        [Sugar( "!=")]
        DIFFERENT = 33,

        [Sugar( ".")] CONCAT = 34,

        [Sugar( ":=")]
        ASSIGN = 35,

        [Sugar( "+")] PLUS = 36,

        [Sugar( "-")] MINUS = 37,


        [Sugar( "*")] TIMES = 38,

        [Sugar( "/")] DIVIDE = 39,

        #endregion

        #region sugar 50 ->

        [Sugar( "(")] LPAREN = 50,

        [Sugar( ")")] RPAREN = 51,

        [Sugar( ";")] SEMICOLON = 52,


        EOF = 0

        #endregion
    }
    
    public enum ShortWhileTokenGeneric
    {
        #region keywords 0 -> 19

        [Keyword("IF")] [Keyword("if")]
        IF = 1,

        [Keyword("THEN")] [Keyword("then")]
        THEN = 2,

        [Keyword("ELSE")] [Keyword("else")]
        ELSE = 3,

        [Keyword("WHILE")] [Keyword("while")]
        WHILE = 4,

        [Sugar("DO")] [Sugar("do")]
        DO = 5,

        [Keyword("SKIP")] [Keyword( "skip")]
        SKIP = 6,

        [Keyword( "TRUE")] [Keyword("true")]
        TRUE = 7,

        [Keyword( "FALSE")] [Keyword( "false")]
        FALSE = 8,

        [Keyword( "NOT")] [Keyword("not")]
        NOT = 9,

        [Keyword( "AND")] [Keyword("and")]
        AND = 10,

        [Keyword( "OR")] [Keyword("or")]
        OR = 11,

        [Keyword( "PRINT")] [Keyword("print")]
        PRINT = 12,

        #endregion

        #region literals 20 -> 29

        [AlphaId] IDENTIFIER = 20,

        [String] STRING = 21,

        [Int] INT = 22,

        #endregion

        #region operators 30 -> 49

        [Sugar( ">")] GREATER = 30,

        [Sugar( "<")] LESSER = 31,

        [Sugar( "==")]
        EQUALS = 32,

        [Sugar( "!=")]
        DIFFERENT = 33,

        [Sugar( ".")] CONCAT = 34,

        [Sugar( ":=")]
        ASSIGN = 35,

        [Sugar( "+")] PLUS = 36,

        [Sugar( "-")] MINUS = 37,


        [Sugar( "*")] TIMES = 38,

        [Sugar( "/")] DIVIDE = 39,

        #endregion

        #region sugar 50 ->

        [Sugar( "(")] LPAREN = 50,

        [Sugar( ")")] RPAREN = 51,

        [Sugar( ";")] SEMICOLON = 52,


        EOF = 0

        #endregion
    }
}