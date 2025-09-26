using sly.lexer;

namespace RelaxedVisitorTyping;

public enum RelaxedExpressionToken
{
    [Sugar("(")]
    LParen,

    [Sugar(")")]
    RParen,


    [CustomId("a-zA-Z","a-zA-Z.")]
    Property,

    [String]
    String,

    [Double]
    Number,

    [Int]
    Int,

    [Sugar("-eq")]
    [Sugar("-equal")]
    Op_Equal,

    [Sugar("-lt")]
    [Sugar("-lowerthan")]
    Op_LowerThan,
    
    [Sugar("-neq")]
    [Sugar("-notequal")]
    Op_NotEqual,

    [Sugar("-gt")]
    [Sugar("-greatethan")]
    Op_GreaterThan,
}