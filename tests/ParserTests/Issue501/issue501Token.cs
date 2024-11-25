using sly.lexer;

namespace ParserTests.Issue501;

public enum Issue501Token
{
    [Keyword("import")]
    [Push("importMode")]
    ImportKeyword,
    [UpTo(";")]
    [Mode("importMode")]
    ImportContent,
    [Sugar(";")]
    [Mode("importMode")]
    [Pop]
    EndImport,

    [AlphaNumDashId]
    Identifier,
    [Int]
    Number,
    [Sugar("=")]
    Assign
}