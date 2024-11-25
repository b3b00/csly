using sly.lexer;

namespace ParserTests.Issue495;

public enum Issue495Token
{
    EOF = 0,

    #region Sugar
    [Comment("//", "/*", "*/")]
    Comment,
    [Sugar("++")]
    Increment,
    [Sugar("--")]
    Decrement,
    [Sugar("+")]
    Plus,
    [Sugar("-")]
    Minus,
    [Sugar("*")]
    Multiply,
    [Sugar("/")]
    Divide,
    [Sugar("%")]
    Remainder,
    [Sugar(">")]
    GreaterThan,
    [Sugar("<")]
    LessThan,
    [Sugar("==")]
    Equal,
    [Sugar("!=")]
    NotEqual,
    [Sugar(">=")]
    GreaterThanOrEqual,
    [Sugar("<=")]
    LessThanOrEqual,
    [Sugar("=")]
    Assign,
    [Sugar("?=")]
    BooleanAssign,
    [Sugar("%=")]
    RemainderAssign,
    [Sugar("+=")]
    PlusAssign,
    [Sugar("-=")]
    MinusAssign,
    [Sugar("*=")]
    MultiplyAssign,
    [Sugar("/=")]
    DivideAssign,
    [Sugar("><")]
    CompareAssign,
    [Sugar("??=")]
    NullColesleAssign,
    [Sugar("=>")]
    Arrow,
    [Sugar("{")]
    BlockStart,
    [Sugar("}")]
    BlockEnd,
    [Sugar("[")]
    ListStart,
    [Sugar("]")]
    ListEnd,
    [Sugar("(")]
    ParenStart,
    [Sugar(")")]
    ParenEnd,
    [Sugar(":")]
    Colon,
    [Sugar("::")]
    Imply,
    [Sugar(",")]
    Comma,
    [Sugar(";")]
    End,
    [Sugar("!")]
    Not,
    [Sugar("$")]
    DollarSign,
    [Sugar(".")]
    Dot,
    [Sugar("&")]
    Deref,
    [Sugar("||")]
    Or,
    [Sugar("&&")]
    And,
    [Sugar("\\")]
    Escape,
    [Sugar("\"")]
    [Push("defaultString")]
    StartQuote,
    [Sugar("\"")]
    [Mode("defaultString")]
    [Pop]
    EndQuote,
    
    [UpTo("\"")]
    [Mode("defaultString")]
    StringValue,
    #endregion

    #region Keywords
    [Keyword("true")]
    True,
    [Keyword("false")]
    False,
    [Keyword("run")]
    RunKeyword,
    [Keyword("import")]
    ImportKeyword,
    [Keyword("function")]
    FunctionKeyword,
    [Keyword("while")]
    WhileKeyword,
    [Keyword("if")]
    IfKeyword,
    [Keyword("else")]
    ElseKeyword,
    [Keyword("do")]
    DoKeyword,
    [Keyword("class")]
    ClassKeyword,
    [Keyword("new")]
    NewKeyword,
    [Keyword("null")]
    NullKeyword,
    [Keyword("command")]
    CommandKeyword,
    [Sugar("@s")]
    SelectorSelf,
    [Sugar("@p")]
    SelectorNearest,
    [Sugar("@a")]
    SelectorAllPlayers,
    [Sugar("@r")]
    SelectorRandomPlayer,
    [Sugar("@e")]
    SelectorAllEntities,
    #endregion

    [Int]
    Int,
    [Double]
    Double,
    [AlphaNumDashId]
    Identifier,

    

    Variable,
    Namespace,
}