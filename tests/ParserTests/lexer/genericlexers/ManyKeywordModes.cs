using sly.lexer;

namespace ParserTests.lexer.genericlexers;

[Lexer]
public enum ManyKeywordModes
{
    [Mode("M1","M2","default")]
    [AlphaNumId] ID,

    [Push("M1")]
    [Mode("M1","M2","default")]
    [Keyword("M1")] M1,

    [Push("M2")]
    [Mode("M1","M2","default")]
    [Keyword("M2")] M2,
        
    [Pop]
    [Mode("M1","M2","default")]
    [Keyword("POP")] POP,

    [Mode("M1")] [Sugar("$")] DOLLAR,
    [Mode("M1")] [Sugar("€")] EURO,

    [Mode("M2")] [Sugar("-")] DASH,
    [Mode("M2")] [Sugar("_")] UNDERSCORE

}