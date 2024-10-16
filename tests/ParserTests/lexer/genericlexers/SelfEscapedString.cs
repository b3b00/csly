using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum SelfEscapedString
{
    [Lexeme(GenericToken.String, "'", "'")]
    STRING
}