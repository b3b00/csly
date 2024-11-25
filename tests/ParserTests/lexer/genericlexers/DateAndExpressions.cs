using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum DateAndExpressions
{

    [Int] INT,

    [Sugar("-")] MINUS,

    [Date(DateFormat.YYYYMMDD, '-')] DATE,
}