using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace rules.model;

public class Statements : IRule
{
    public Statements(IEnumerable<IStatement> statements, Position position)
    {
        Position = position;
        Stats = statements.ToList();
    }

    public List<IStatement> Stats { get; }

    [ExcludeFromCodeCoverage]
    public string Dump()
    {
        var b = new StringBuilder();
        foreach (var stat in Stats)
        {
            if (stat != null)
            {
                b.AppendLine(stat.Dump());
            }
        }

        return b.ToString();
    }

    public Position Position { get; set; }
}
