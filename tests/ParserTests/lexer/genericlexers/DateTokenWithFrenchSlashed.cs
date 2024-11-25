using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum DateTokenWithFrenchSlashed
{
    [Date(DateFormat.DDMMYYYY, '/')] DATE,

    [Int] INT
}