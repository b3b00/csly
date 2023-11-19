using sly.lexer;

namespace ParserTests.Issue311;

public enum Token311
{
    NOT_A_TOKEN = 0,
    [String("'", "'")] STRING = 1,
    [Double(".")] DOUBLE = 2,
    [Keyword("eq")] EQ = 3,
}

public enum Token311ComaDecimal
{
    NOT_A_TOKEN = 0,
    [String("'", "'")] STRING = 1,
    [Double(",")] DOUBLE = 2,
    [Keyword("eq")] EQ = 3,
}