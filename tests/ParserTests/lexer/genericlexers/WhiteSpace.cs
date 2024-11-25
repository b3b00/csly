using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer(WhiteSpace = new[] { ' ' })]
public enum WhiteSpace
{
    [Lexeme(GenericToken.SugarToken, "\t")]
    TAB
}