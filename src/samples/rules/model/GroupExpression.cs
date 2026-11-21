using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace rules.model;

public class GroupExprpression : RuleExpression
{
    public GroupExprpression(RuleExpression expression, Position position)
    {
        Position = position;

        Expression = expression;
    }

    public RuleExpression Expression { get; }

    public RuleExpression Right { get; }


    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        return $"( {Expression.Dump()} )";
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
