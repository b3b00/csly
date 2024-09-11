// ReSharper disable InconsistentNaming
namespace ParserTests.Issue225_IndexOutOfRangeException
{
    public enum Issue223OorDateOffsetKind
    {
        SECOND = 1,
        MINUTE = SECOND * 60,
        HOUR = MINUTE * 60,
        DAY = HOUR * 24,
        WEEK = DAY * 7,
        MONTH = DAY * 30,
        YEAR = DAY * 365
    }
}