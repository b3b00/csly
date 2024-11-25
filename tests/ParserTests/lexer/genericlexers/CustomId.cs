using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum CustomId
{
    EOS,

    [CustomId("A-Za-z", "-_0-9A-Za-z")]
    // [Lexeme(GenericToken.Identifier, IdentifierType.Custom, "A-Za-z", "-_0-9A-Za-z")]
    ID,

    [Lexeme(GenericToken.SugarToken, "-", "_")]
    OTHER
}