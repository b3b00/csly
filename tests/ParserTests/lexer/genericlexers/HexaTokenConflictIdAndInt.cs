using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum HexaTokenConflictIdAndInt
{
    [Hexa("0x")]HEXA,
        
    [AlphaId] ID,
        
    [Int] INT,
}