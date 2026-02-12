using sly.lexer;

namespace ParserTests.ambiguous;

public enum AmbiguousToken
{
    [Lexeme("a")] A,
    EOF
}