using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum HexaTokenConflictId
{
    EOS= 0,
        
        
    [Hexa("hexa")]HEXA,
        
    [AlphaId] ID
}