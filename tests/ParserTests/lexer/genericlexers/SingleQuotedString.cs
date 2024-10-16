using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum SingleQuotedString
{
    [Lexeme(GenericToken.String, "'")] SingleString
}