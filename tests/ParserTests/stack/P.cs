using sly.lexer;

namespace ParserTests.stack;

public enum P
{
    [Sugar("+")] e,
    [Keyword("true")] True,
    [Keyword("false")] False,
}