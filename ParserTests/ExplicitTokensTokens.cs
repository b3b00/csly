using sly.lexer;

namespace ParserTests
{
    [Lexer(IgnoreWS = true, IgnoreEOL = true)]
    public enum ExplicitTokensTokens
    {
        [MultiLineComment("/*","*/")]
        MULTILINECOMMENT = 1,
        
        [SingleLineComment("//")]
        SINGLELINECOMMENT = 2,

        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumeric)]
        ID = 3,
        
        [Lexeme(GenericToken.Double, channel:101)]
        DOUBLE = 4,
        
        [Keyword("Test")] 
        TEST = 5,
        [Sugar("*")]
        TIMES = 6,
        
        [Sugar("/")]
        DIVIDE = 7
    }
}