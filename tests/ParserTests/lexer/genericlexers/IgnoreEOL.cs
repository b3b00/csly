using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer(IgnoreEOL = false)]
public enum IgnoreEOL
{
    [Lexeme(GenericToken.SugarToken, "\n")]
    EOL
}