using sly.lexer;

namespace ParserTests.Issue443;

public enum Test443Lexer
{
    

    [Sugar("@")]
    A,
    [Sugar("$")]
    B
}