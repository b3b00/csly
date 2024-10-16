using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum Empty
{
    EOS,

    [Lexeme(GenericToken.Identifier)] ID
}