using sly.lexer;

namespace ParserExample;

public enum TestGrammarToken
{
    [Lexeme(GenericToken.SugarToken,",")]
    COMMA = 1
}