using sly.lexer;

namespace ParserTests.lexer.genericlexers
{
    public enum ShortExtensions
    {
        [Extension] DATE,

        [Double] DOUBLE,
        
        [Extension]
        TEST
        
    }
}