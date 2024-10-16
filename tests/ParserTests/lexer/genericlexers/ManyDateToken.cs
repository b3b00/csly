using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum ManyDateToken
{
    [Date(DateFormat.DDMMYYYY, '/')] FRENCH_DATE,

    [Date(DateFormat.YYYYMMDD, '-')] ENGLISH_DATE,
}