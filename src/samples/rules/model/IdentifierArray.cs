using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace rules.model;

public class IdentifierArray : RuleExpression
{
    public IdentifierArray(List<Identifier> identifiers)
    {
        Position = identifiers.First().Position;
        Identifiers = identifiers;
    }

    public List<Identifier> Identifiers { get; set; }

    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("[ ");
        builder.Append(string.Join(", ", Identifiers.Select(x => x.Dump())));
        builder.Append(" ]");
        return builder.ToString();
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
            if (obj is IdentifierArray array)
            {
                if (array.Identifiers.Count == Identifiers.Count)
                {
                    bool ok = true;
                    int i = 0;
                    while (ok && i < array.Identifiers.Count)
                    {
                        ok = ok && array.Identifiers[i].Equals(Identifiers[i]);
                        i++;
                    }
                    return ok;
                }
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
