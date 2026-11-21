using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace rules.model;

public class StringLiteral : Literal
{
    public StringLiteral(string value, Position position)
    {
        Position = position;
        Value = value;
    }

    public string Value { get; }

    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        return $@"""{Value}""";
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
