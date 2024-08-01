using sly.lexer;

namespace ParserTests.Discussion468
{
    public enum Discussion468Lexer
    {
        [CustomId("_.:a-zA-Z","_.:0-9a-zA-Z")]
        INDENTIFIER

    }
}