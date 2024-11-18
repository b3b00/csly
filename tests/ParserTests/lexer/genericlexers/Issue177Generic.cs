using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer(IgnoreEOL = false)]
public enum Issue177Generic
{

    [Sugar("\r\n",IsLineEnding = true)]
    [Sugar("\n",IsLineEnding = true)]
    EOL = 1,
    
    [Int] INT = 2,
        

    EOS = 0

}