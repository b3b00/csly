using sly.lexer;

namespace ParserTests.Issue443;

[Lexer(IgnoreEOL = false)]
public enum Issue443Lexer
{
    EOF = 0,

    [Sugar("\r\n", IsLineEnding = true)]
    [Mode]
    CRLF,
    [Sugar("\n",  IsLineEnding = true)]
    [Mode]
    LF
}