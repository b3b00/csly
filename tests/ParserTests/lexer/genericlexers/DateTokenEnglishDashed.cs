using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum DateTokenEnglishDashed
{
    [Date(DateFormat.YYYYMMDD, '-')] DATE,
}