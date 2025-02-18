using sly.lexer;

namespace ParserTests.Issue536;

public enum Token536
{
    EOF,
    [Sugar("=")]
    Equals,
    [Sugar("-")]
    Plus
}