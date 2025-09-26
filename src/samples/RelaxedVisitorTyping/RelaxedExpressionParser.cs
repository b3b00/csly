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

    [Production("primary: Number")]
    public Double Primary_Number(Token<RelaxedExpressionToken> numberToken)
    {
        var value = numberToken.DoubleValue;
        return value;
    }

    [Production("primary: String")]
    public string Primary_String(Token<RelaxedExpressionToken> stringToken)
    {
        var value = stringToken.Value;
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
    public Clause Compare(Token<RelaxedExpressionToken> property, string op, object primary)
    {
        return new Clause()
        {
            Property = property.Value,
            Op = op,
            Value = primary,
        };
    }
}

public class EbnfRelaxedExpressionParser
{
    [Production("primary: Int")]
    public int primaryInt(Token<RelaxedExpressionToken> integer) => integer.IntValue;

    [Production("many : primary*")]
    public List<int> Many(List<int> integers) => integers;

}