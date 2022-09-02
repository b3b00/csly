using sly.lexer;

namespace ParserTests.Issue311;

public enum Token311
{
    [String("'", "'")] STRING,
    [Double] DOUBLE,
    [Keyword("eq")] EQ,
}

