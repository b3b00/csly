using sly.lexer;

namespace ParserExample
{
    public enum ScriptToken
    {
        EOS = 0,
        [Lexeme(GenericToken.SugarToken,",")]
        COMMA = 1,
        [Lexeme(GenericToken.SugarToken,"(")]
        LPAR = 2,
        [Lexeme(GenericToken.SugarToken,")")]
        RPAR = 3,
        [Lexeme(GenericToken.SugarToken,"=")]
        DEFINE = 4,
        [Lexeme(GenericToken.Identifier,IdentifierType.AlphaNumeric)]
        ID = 5,
        [Lexeme(GenericToken.SugarToken,"|B|")]
        LBEG = 6,
        [Lexeme(GenericToken.SugarToken,"|E|")]
        LEND = 7,
        [Lexeme(GenericToken.Int)]
        INT = 8,
        [Lexeme(GenericToken.String,"\"","\\")]
        STRING = 9
    }
}