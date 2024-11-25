using sly.lexer;

namespace ParserTests.Issue485;

public enum Issue485Lexer
{
    [Sugar(":")] COLON,
    [Keyword("Property")] PROPERTY, 
    [String] STRING
}