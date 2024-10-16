using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum DoubleQuotedString
{
    [Lexeme(GenericToken.String, "\"")] DoubleString
}