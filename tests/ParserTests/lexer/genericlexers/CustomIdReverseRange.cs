using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum CustomIdReverseRange
{
    EOS,

    [CustomId("Z-Az-a", "-_9-0A-Za-z")] ID,

    [Lexeme(GenericToken.SugarToken, "-", "_")]
    OTHER
}