using sly.lexer;

namespace ParserTests.Issue485;

public enum Issue485SelfEscapeLexer
{
    [Sugar(":")] COLON,
    [Keyword("Property")] PROPERTY, 
    [String("\"","\"")] STRING
}