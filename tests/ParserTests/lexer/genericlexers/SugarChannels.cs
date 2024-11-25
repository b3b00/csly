using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer]
public enum SugarChannelsToken
{
    [Sugar("$",42)]
    DOLLAR,
    
    [Sugar("€",84)]
    EURO
    
}