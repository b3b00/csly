using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer(IgnoreWS = false)]
public enum IgnoreWS
{
    [Lexeme(GenericToken.SugarToken, " ")] WS
}