using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace rules.model;

public sealed class DoubleLiteral : Literal
{
    public DoubleLiteral(decimal value, Position position)
    {
        Position = position;
        Value = value;
    }

    public decimal Value { get; }

    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        return Value.ToString();
    }

    public override IEnumerable<Identifier> GetPropertyReferences()
    {
        return Enumerable.Empty<Identifier>();
    }

    public override IEnumerable<Identifier> GetSettedReferences()
    {
        return Enumerable.Empty<Identifier>();
    }

    public override bool Equals(object obj)
    {
        if (obj != null)
        {
            if (obj is DoubleLiteral d)
            {
                return d.Value == Value;
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
