using sly.lexer;
using sly.parser.generator;

namespace RelaxedVisitorTyping;

internal class RelaxedExpressionParser
{
    [Production("primary: Int")]
    public Int32 Primary_Int(Token<RelaxedExpressionToken> intToken)
    {
        return intToken.IntValue;
    }

    [Production("primary: Number")]
    public Double Primary_Number(Token<RelaxedExpressionToken> numberToken)
    {
        return numberToken.DoubleValue;
    }

    [Production("primary: String")]
    public string Primary_String(Token<RelaxedExpressionToken> stringToken)
    {
        return stringToken.Value;
    }

    [Production("op: Op_Equal")]
    [Production("op: Op_LowerThan")]
    public string Op(Token<RelaxedExpressionToken> stringToken)
    {
        return stringToken.Value.Skip(1).ToString();
    }

    [Production("compare: Property op primary")]
    public Clause Compare(Token<RelaxedExpressionToken> property, string op, object primary)
    {
        return new Clause()
        {
            Property = property.Value,
            Op = op as string,
            Value = primary,
        };
    }
}