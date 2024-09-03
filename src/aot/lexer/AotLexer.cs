using sly.lexer;

namespace aot.lexer;

[Lexer(IgnoreWS = true, KeyWordIgnoreCase = true, IndentationAWare = false, WhiteSpace = new[]{' ','\t'}, IgnoreEOL = true,  Indentation = "\t")]
public enum AotLexer
{
    [Lexeme("$-$")]
    PATTERN,
    
    [Double]
    DOUBLE,
    [AlphaId]
    IDENTIFIER,
    [Sugar("+")]
    PLUS,
    [Sugar("++")]
    INCREMENT,
    [Sugar("-")]
    MINUS,
    [Sugar("*")]
    TIMES,
    [Sugar("/")]
    DIVIDE,
    [Sugar("(")]
    LPAREN,
    [Sugar(")")]
    RPAREN,
    [Sugar("!")]
    FACTORIAL,
    [Sugar("²")]
    SQUARE,
    [SingleLineComment("//")]
    SINGLELINECOMMENT,
    [MultiLineComment("/*","*/")]
    MULTILINECOMMENT,
}