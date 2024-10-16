using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer(IgnoreEOL = true)]
public enum Issue177Generic
{

    [Lexeme(GenericToken.Int)] INT = 2,

    EOS = 0

}