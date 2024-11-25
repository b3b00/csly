using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer]
public enum Issue186MixedGenericAndRegexLexer
{
    [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
    ID = 1,

    [Lexeme("[0-9]+")] INT = 2
}