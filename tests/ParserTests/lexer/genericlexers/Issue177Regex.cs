using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer(IgnoreEOL = false)]
public enum Issue177Regex
{
    [Lexeme("\r\n", IsLineEnding = true)] EOL = 1,

    [Lexeme("\\d+")] INT = 2,

    EOS = 0

}