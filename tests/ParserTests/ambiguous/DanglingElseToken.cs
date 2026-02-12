using sly.lexer;
using sly.lexer.fsm;
using sly.i18n;

namespace ParserTests.ambiguous
{
    public enum DanglingElseToken
    {
        [AlphaId]
        ID,
        [Int]
        INT,
        [Keyword("if")]
        IF,
        [Keyword("else")]
        ELSE,
        [Keyword("then")]
        THEN,
        [Sugar("==")]
        EQUALS,
        [Sugar(":=")]
        ASSIGN,
    }
}