using sly.lexer;

namespace csly.indentedWhileLang.parser
{
    [Lexer(IndentationAWare = true, Indentation = "\t")]
    public enum IndentedWhileTokenGeneric
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

        [Lexeme(GenericToken.KeyWord, "RETURN")] [Lexeme(GenericToken.KeyWord, "return")]
        RETURN = 13,

        #endregion

        #region literals 20 -> 29

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumericDash)]
        IDENTIFIER = 20,

        [Lexeme(GenericToken.Int)] INT = 22,

        #endregion

        #region operators 30 -> 49

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, ">")] GREATER = 30,

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, "<")] LESSER = 31,

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, "==")]
        EQUALS = 32,

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, "!=")]
        DIFFERENT = 33,

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, ".")] CONCAT = 34,

        
        [Lexeme(GenericToken.SugarToken, ":=")] ASSIGN = 35,

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, "+")] PLUS = 36,

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, "-")] MINUS = 37,

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, "*")] TIMES = 38,

        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Lexeme(GenericToken.SugarToken, "/")] DIVIDE = 39,
        
        [Mode("default","fstringExpression")]
        [Sugar("?")]
        QUESTION,
        
        [Mode("default","fstringExpression")]
        [Sugar("->")]
        ARROW,
        
        [Mode("default","fstringExpression")]
        [Sugar("(")]
        OPEN_PAREN,
        
        [Mode("default","fstringExpression")]
        [Sugar(")")]
        CLOSE_PAREN,
        
        [Mode("default","fstringExpression")]
        [Sugar("|")]
        COLON,
        
        #endregion

        #region sugar 50 ->


        [Lexeme(GenericToken.SugarToken, ";")] SEMICOLON = 52,


        [SingleLineComment("#")] COMMENT = 1236,

        #endregion

        #region fstring 100 ->

        [Push("fstringExpression")] 
        [Mode("fstring")]
        [Sugar("{")]
        OPEN_FSTRING_EXPPRESSION = 100,

        [Pop] 
        [Mode("fstringExpression")] 
        [Sugar("}")]
        CLOSE_FSTRING_EXPPRESSION = 101,

        [Sugar("$\"")]
        [Mode(ModeAttribute.DefaultLexerMode, "fstringExpression")]
        [Push("fstring")] 
        OPEN_FSTRING,

        [Sugar("\"")] 
        [Mode("fstring", "fstringExpression")]
        [Pop]  
        CLOSE_FSTRING,

        
        [Mode("fstring")]
        [UpTo("{","\"")]
        FSTRING_CONTENT,
        
        
        
        
        
        
        
        #endregion



    }
}