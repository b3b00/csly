using sly.lexer;

namespace indented
{
    [Lexer(Indentation = "    ",IndentationAWare = true, IgnoreEOL = false)]
    public enum IndentedLangLexer2
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
        ID = 1,

        [Lexeme(GenericToken.KeyWord, "if")] IF = 2,

        [Lexeme(GenericToken.KeyWord, "else")] ELSE = 3,

        [Lexeme(GenericToken.SugarToken, "==")] EQ = 4,

        [Lexeme(GenericToken.SugarToken, "=")] SET = 5,

        [Lexeme(GenericToken.Int)] INT = 6,

        [Lexeme(GenericToken.SugarToken, "\n",IsLineEnding = true)]
        [Lexeme(GenericToken.SugarToken, "\r\n",IsLineEnding = true)]
        [Lexeme(GenericToken.SugarToken, "\r",IsLineEnding = true)]
        EOL = 7,
        
        [SingleLineComment("//")]
        SINGLE_COMMENT = 100,
        
        [MultiLineComment("/*","*/")]
        MULTI_COMMENT = 101

    }
}