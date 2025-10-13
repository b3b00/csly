using sly.lexer;
using sly.parser.generator;

namespace RelaxedVisitorTyping;

public class RelaxedExpressionParser
{
    [Production("primary: Int")]
    public Int32 Primary_Int(Token<RelaxedExpressionToken> intToken)
    {
        var value = intToken.IntValue;
        return value;
    }

    [Production("op: Op_Equal")]
    [Production("op: Op_LowerThan")]
    [Production("op: Op_GreaterThan")]
    [Production("op: Op_NotEqual")]
    public string Op(Token<RelaxedExpressionToken> stringToken)
    {
        var op = stringToken.Value.Substring(1);
        return op;
    }

    [Production("compare: Property op primary")]
    public Clause Compare(Token<RelaxedExpressionToken> property, string op, int primary)
    {
        return new Clause()
        {
            Property = property.Value,
            Op = op,
            Value = primary,
        };
    }
}