using sly.lexer;

namespace ParserTests.Issue447;

[Lexer(KeyWordIgnoreCase = true)]
public enum Issue447Lexer
{
    EOS = 0,
    [Keyword("A")]
    A,
}