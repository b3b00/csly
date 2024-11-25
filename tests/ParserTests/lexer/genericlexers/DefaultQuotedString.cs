using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum DefaultQuotedString
{
    [Lexeme(GenericToken.String)] DefaultString
}