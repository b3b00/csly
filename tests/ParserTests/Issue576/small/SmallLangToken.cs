using sly.lexer;

namespace Common.Tokens;

public enum SmallLangToken
{
    EOF,

    //should be shared between all sets of valid tokens, since invalid ones will simply be unused
    [Lexeme(GenericToken.Int)]
    [Lexeme(GenericToken.Double)]
    Number,

    [Lexeme(GenericToken.Identifier, IdentifierType.Custom, "_A-Za-z", "_0-9A-Za-z")]
    Identifier,

    [Lexeme(GenericToken.String)]
    [Lexeme(GenericToken.String, "'")]
    String,

    //Operators

    #region bitwise operator

    [Sugar("&")] BitwiseAnd,
    [Sugar("|")] BitwiseOr,
    [Sugar("^")] BitwiseXor,
    [Sugar("<<")] BitwiseLeftShift,
    [Sugar(">>")] BitwiseRightShift,
    [Sugar("~")] BitwiseNegation,

    #endregion

    #region arithmetic operator

    [Sugar("+")] Addition,
    [Sugar("-")] Subtraction,
    [Sugar("*")] Multiplication,
    [Sugar("/")] Division,
    [Sugar("**")] Exponentiation,
    [Sugar("!")] Factorial,

    #endregion

    #region sugar immediate assign

    [Sugar("+=")] PlusEquals,
    [Sugar("-=")] MinusEquals,
    [Sugar("*=")] MultiplicationEquals,
    [Sugar("/=")] DivideEquals,
    [Sugar("**=")] PowerEquals,
    [Sugar("&=")] BitwiseAndEquals,
    [Sugar("|=")] BitwiseOrEquals,
    [Sugar("^=")] BitwiseXorEquals,
    [Sugar("<<=")] LeftShiftEquals,
    [Sugar(">>=")] RightShiftEquals,
    [Sugar("~=")] BitwiseNegateEquals,
    [Sugar("++")] Increment,
    [Sugar("--")] Decrement,

    #endregion

    #region logical operator

    [Keyword("and")] LogicalAnd,
    [Keyword("or")] LogicalOr,
    [Keyword("not")] LogicalNot,
    [Keyword("xor")] LogicalXor,
    [Keyword("implies")] LogicalImplies,

    #endregion

    #region Brackets

    [Sugar("(")] OpenParen,
    [Sugar(")")] CloseParen,
    [Sugar("{")] OpenCurly,
    [Sugar("}")] CloseCurly,
    [Sugar("[")] OpenSquare,
    [Sugar("]")] CloseSquare,
    [Sugar("<[")] OpenAngleSquare,
    [Sugar("]>")] CloseAngleSquare,

    #endregion

    #region Symbols

    [Sugar(",")] Comma,
    [Sugar(":")] Colon,
    [Sugar(".")] Dot,

    #endregion

    #region Comparison_Operator

    [Sugar("==")] EqualTo,
    [Sugar("!=")] NotEqualTo,
    [Sugar(">")] GreaterThan,
    [Sugar("<")] LessThan,
    [Sugar(">=")] GreaterThanOrEqualTo,
    [Sugar("<=")] LessThanOrEqualTo,
    [Keyword("is")] ComparisonIs,
    [Keyword("in")] In,
    [Sugar(";")] Semicolon,

    #endregion

    [Sugar("=")] Equals,

    #region Types

    [Keyword("float")] TypeFloat,
    [Keyword("int")] TypeInt,
    [Keyword("double")] TypeDouble,

    [Keyword("number")]
    [Keyword("bigfloat")]
    TypeNumber,
    [Keyword("long")] TypeLong,

    [Keyword("longint")]
    [Keyword("bigint")]
    TypeLongInt,
    [Keyword("byte")] TypeByte,
    [Keyword("array")] TypeArray,
    [Keyword("list")] TypeList,
    [Keyword("set")] TypeSet,
    [Keyword("dict")] TypeDict,
    [Keyword("short")] TypeShort,
    [Keyword("rational")] TypeRational,
    [Keyword("string")] TypeString,
    [Keyword("char")] TypeChar,
    [Keyword("void")] TypeVoid,
    [Keyword("bool")] TypeBool,

    #endregion

    #region control-flow

    [Keyword("return")] Return,
    [Keyword("break")] Break,
    [Keyword("continue")] Continue,
    [Keyword("if")] If,
    [Keyword("else")] Else,
    [Keyword("switch")] Switch,

    #endregion

    #region Loops

    [Keyword("for")] For,
    [Keyword("while")] While,

    #endregion

    [Keyword("as")] As,

    #region FunctionMod

    [Keyword("cascading")] Cascading,
    [Keyword("copy")] Copy,
    [Keyword("readonly")] Readonly,
    [Keyword("frozen")] Frozen,
    [Keyword("immut")] Immut,
    [Keyword("ref")] Ref,
    [Keyword("new")] New,

    #endregion

    [Keyword("true")] TrueLiteral,
    [Keyword("false")] FalseLiteral,
    [Keyword("collection")] TypeCollection,
    [Keyword("pass")] Pass,

    [Comment("#", "/*", "*/")] Comment
}