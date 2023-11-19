using sly.lexer;

namespace indented
{
    [Lexer(IndentationAWare = true)]
    public enum IndentedLangLexer
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
        ID = 1,

        [Lexeme(GenericToken.KeyWord, "if")] IF = 2,

        [Lexeme(GenericToken.KeyWord, "else")] ELSE = 3,

        [Lexeme(GenericToken.SugarToken, "==")] EQ = 4,

        [Lexeme(GenericToken.SugarToken, "=")] SET = 5,

        [Lexeme(GenericToken.Int)] INT = 6,
         
        [SingleLineComment("//")] SINGLE_COMMENT = 8,
        
        [MultiLineComment("/*","*/")] MULTI_COMMENT = 9,

    }
}