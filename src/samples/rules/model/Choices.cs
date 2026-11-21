using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace rules.model;

public class Choices : RuleExpression
{
    public Choices(List<Literal> choices)
    {
        Values = choices;
    }

    public Choices(Literal head, IEnumerable<Literal> tail, Position position)
    {
        Position = position;
        Values = new List<Literal>();
        Values.Add(head);
        Values.AddRange(tail);
    }

    public List<Literal> Values { get; set; }

    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        var b = new StringBuilder();
        b.Append("{ ");
        foreach (var value in Values)
        {
            if (value != null)
            {
                b.Append($"{value.Dump()}, ");
            }
        }

        b.Append(" }");
        return b.ToString();
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
