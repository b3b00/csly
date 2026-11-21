using System.Collections.Generic;
using System.Linq;

namespace rules.model;

public class UnaryMinusExpression : RuleExpression, IRule
{
    public UnaryMinusExpression(RuleExpression expression, Position position)
    {
        Position = position;
        Expression = expression;
    }

    public RuleExpression Expression { get; }

    public override string Dump()
    {
        return $"- {Expression.Dump()}";
    }

    public override IEnumerable<Identifier> GetPropertyReferences()
    {
        return Expression.GetPropertyReferences();
    }

    public override IEnumerable<Identifier> GetSettedReferences()
    {
        return Enumerable.Empty<Identifier>();
    }
}
