using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace rules.model;

public class In : RuleExpression
{
    public In(Identifier left, Choices right, Position position)
    {
        Position = position;
        Identifier = left;
        Right = right;
    }

    public Identifier Identifier { get; }

    public IRule Right { get; }

    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        return $"{Identifier.Dump()} DANS {Right.Dump()}";
    }

    public override IEnumerable<Identifier> GetPropertyReferences()
    {
        var result = new List<Identifier> { Identifier };
        return result;
    }

    public override IEnumerable<Identifier> GetSettedReferences()
    {
        return Enumerable.Empty<Identifier>();
    }
}
