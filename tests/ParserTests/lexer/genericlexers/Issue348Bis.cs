using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum Issue348Bis
{
    [Lexeme(GenericToken.String, "\"", "^")]
    STriNG
}