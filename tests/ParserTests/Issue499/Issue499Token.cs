using sly.lexer;

namespace ParserTests.Issue499;
internal enum Issue499Token
{
    EOF = 0,

    [UpTo("\"", Channel = 1)]
    Test,
    [Sugar("\"", Channel = 0)]
    Quote
}
