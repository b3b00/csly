using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace rules.model;

public class BoolLiteral : Literal
{
    public BoolLiteral(bool value, Position position)
    {
        Position = position;
        Value = value;
    }

    public bool Value { get; }

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
}
