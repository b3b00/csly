using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace rules.model;

public class FunctionCall : RuleExpression
{
    public FunctionCall(string functionName, List<RuleExpression> args, Position position)
    {
        Position = position;
        FunctionName = functionName;
        Arguments = args;
    }

    public string FunctionName { get; }

    public List<RuleExpression> Arguments { get; set; }

    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        var builder = new StringBuilder();
        builder.Append(FunctionName).Append("(");
        for (var i = 0; i < Arguments.Count; i++)
        {
            builder.Append(Arguments[i].Dump());
            if (i < Arguments.Count - 1)
            {
                builder.Append(", ");
            }
        }

        builder.Append(")");
        return builder.ToString();
    }

    public override IEnumerable<Identifier> GetPropertyReferences()
    {
        return Arguments.SelectMany(x => x.GetPropertyReferences());
    }

    public override IEnumerable<Identifier> GetSettedReferences()
    {
        return Enumerable.Empty<Identifier>();
    }
}
