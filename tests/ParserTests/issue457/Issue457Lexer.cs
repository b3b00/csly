using sly.lexer;

namespace ParserTests.issue457;

public enum Issue457Lexer
{
    [Int] INT,
    [Double] DOUBLE,
    [Hexa] HEXA,

    [String("\"", "\\")]
    [String("'", "\\")]
    STRING,

    [SingleLineComment("#")]
    [MultiLineComment("\"\"\"", "\"\"\"")] 
    COMMENT
}