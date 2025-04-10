using sly.lexer;

namespace ParserTests.dateIssue;

public enum Issue554lexer
{
    [Date(DateFormat.YYYYMMDD, '.')]
    DATE,
    [Double]
    DOUBLE,
    [Int]
    INT,
}