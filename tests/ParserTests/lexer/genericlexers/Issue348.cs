using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum Issue348
{
    [Lexeme(GenericToken.String, "\"", "\\")]
    STriNG
}