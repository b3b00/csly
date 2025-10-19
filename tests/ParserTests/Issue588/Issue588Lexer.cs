using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue588;

[Lexer(IndentationAWare =true)]

public enum Issue588Lexer {
    EOS,

    [Keyword("if")]
    IF,

    [Keyword("else")]
    ELSE,

    [Sugar("=")]
    SET,

    [Sugar("==")]
    EQ,



    [AlphaId]
    ID,

    [Int]
    INT,


}
