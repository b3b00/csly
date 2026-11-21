using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace rules.model;

public class AssignmentStatement : IStatement
{
    public AssignmentStatement(RuleExpression left, IRule expr, Position position)
    {
        Property = left;
        Expression = expr as RuleExpression;
        Position = position;
    }

    public List<RuleComment> PrecedingComments { get; set; }

    public RuleExpression Property { get; }

    public RuleExpression Expression { get; }

    public Position Position { get; set; }

    [ExcludeFromCodeCoverage]
    public string Dump()
    {
        return $"{Property.Dump()} = {Expression.Dump()}";
    }

    public IEnumerable<Identifier> GetPropertyReferences()
    {
        var refs = new List<Identifier>();
        refs.AddRange(Property.GetPropertyReferences());
        refs.AddRange(Expression.GetPropertyReferences());
        return refs;
    }

    public IEnumerable<Identifier> GetSettedReferences()
    {
        if (Expression is DoubleLiteral dbl && dbl.Value == 0.0m || Expression is IntLiteral i && i.Value == 0)
        {
            return Enumerable.Empty<Identifier>();
        }
        return Property.GetPropertyReferences();
    }
}
