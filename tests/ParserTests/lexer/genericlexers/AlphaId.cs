using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum AlphaId
{
    [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
    ID
}