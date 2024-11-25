using sly.lexer;

namespace ParserTests.lexer.genericlexers;

public enum ManyString
{
    [Lexeme(GenericToken.String, "'", "'")] [Lexeme(GenericToken.String)]
    STRING
}