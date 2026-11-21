using System.Collections.Generic;

namespace rules.model;

public abstract class RuleExpression : IRule
{

    public bool IsIdentifier { get; set; } = false;

    public abstract string Dump();

    public Position Position { get; set; }

    public List<RuleComment> PrecedingComments { get; set; }

    public abstract IEnumerable<Identifier> GetPropertyReferences();

    public abstract IEnumerable<Identifier> GetSettedReferences();
}
