using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum MixedGenericRegexLexer
    {
        [Lexeme("[0-9]*")]
        REGEX_INT = 0,
        
        [Lexeme(GenericToken.Double)]
        GENERIC_DOUBLE = 1,
        
        [String(@"""","\\")]
        GENERIC_STRING
    }
}