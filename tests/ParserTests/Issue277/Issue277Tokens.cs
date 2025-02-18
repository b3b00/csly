using sly.lexer;

namespace ParserTests.Issue277;

public enum Issue277Tokens
{
    [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumericDash)]
    IDENTIFIER,

    [Lexeme(GenericToken.KeyWord, "or")]
    OR
}