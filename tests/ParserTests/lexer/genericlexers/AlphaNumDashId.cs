using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum AlphaNumDashId
{
    [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumericDash)]
    ID
}