using sly.lexer;
using sly.lexer.fsm;
using sly.i18n;

namespace ParserTests.Issue574
{
    public enum TokenIssue574
    {
        [CustomId("_A-Za-z", "_0-9A-Za-z")]
        Identifier,
        [Sugar(",")]
        Comma,
        [Keyword("specifier1")]
        Specifier1,
        [Keyword("specifier2")]
        Specifier2,
        [Keyword("type")]
        Type,
    }
}