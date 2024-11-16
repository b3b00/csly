using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer(IgnoreEOL = false)]
public enum Issue177Generic
{

    [Sugar("\r\n")]
    EOL = 1,
    
    [Int] INT = 2,
        

    EOS = 0

}