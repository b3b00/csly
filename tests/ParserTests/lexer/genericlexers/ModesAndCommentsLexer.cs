using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum ModesAndCommentsLexer
{
    [Mode("default")]
    [SingleLineComment("#")]
    SINGLELINE = 1,
        
    [Mode("default","IN")]
    [MultiLineComment("/*","*/")]
    MULTILINE = 2,
        
    [Mode("default","IN")]
    [AlphaId]
    ID = 3,
        
    [Push("IN")]
    [Mode]
    [Sugar(">>>")]
    IN = 4,
        
    [Pop]
    [Mode("IN")]
    [Sugar("<<<")]
    OUT = 5,
        
    [Mode("IN")]
    [Keyword("toto")]
    TOTO = 6
}