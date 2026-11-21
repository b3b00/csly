using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace rules.model;

public class StringArray : RuleExpression
{
    public StringArray(List<StringLiteral> strings)
    {
        Position = strings.First().Position;
        Strings = strings;
    }

    public List<StringLiteral> Strings { get; set; }


    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("[ ");
        builder.Append(string.Join(", ", Strings.Select(x => '"'+x.Dump()+'"')));
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
            if (obj is StringArray array)
            {
                if (array.Strings.Count == Strings.Count)
                {
                    bool ok = true;
                    int i = 0;
                    while (ok && i < array.Strings.Count)
                    {
                        ok = ok && array.Strings[i].Equals(Strings[i]);
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
