using System.Collections.Generic;
using System.Linq;

namespace rules.model;

public class ArgList : IRule
{
    public ArgList(List<RuleExpression> arguments)
    {
        Arguments = arguments;
    }

    public List<RuleExpression> Arguments { get; }

    public Position Position { get; set; }

    public string Dump()
    {
        return string.Join(',', Arguments.Select(x => x.Dump()));
    }

    public IEnumerable<Identifier> GetPropertyReferences()
    {
        return Arguments.SelectMany(x => x.GetPropertyReferences());
    }

    public IEnumerable<Identifier> GetSettedReferences()
    {
        return Enumerable.Empty<Identifier>();
    }
}
