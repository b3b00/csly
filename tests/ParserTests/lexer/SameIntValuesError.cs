using sly.lexer;

namespace ParserTests.lexer
{
    public enum SameIntValuesError
    {
        [Lexeme(GenericToken.Identifier,IdentifierType.Alpha)] ID = 1,
        
        [Lexeme(GenericToken.KeyWord,"Keyword1")]
        keyword1 = 2,

        [Lexeme(GenericToken.KeyWord,"Keyword2")]
        keyword2 = 2
    }
}