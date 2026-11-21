using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace rules.model;


public class BooleanExpr : RuleExpression
{
    public BooleanExpr(BooleanOperation operation, RuleExpression left, RuleExpression right,
        Position position)
    {
        Position = position;
        Operation = operation;
        Left = left;
        Right = right;
    }

    public BooleanOperation Operation { get; }

   

    public RuleExpression Left { get; }

    public RuleExpression Right { get; }

    [ExcludeFromCodeCoverage]
    public string OperatorStringRepresentation()
    {
        switch (Operation)
        {
            case BooleanOperation.IS:
                {
                    return "IS";
                }
            case BooleanOperation.OR:
                {
                    return "OR";
                }
            case BooleanOperation.AND:
                {
                    return "AND";
                }
        }

        return "";
    }

    [ExcludeFromCodeCoverage]
    public override string Dump()
    {
        var op = OperatorStringRepresentation();

        return $"{Left.Dump()} {op} {Right.Dump()}";
    }

    public override IEnumerable<Identifier> GetPropertyReferences()
    {
        var ids = new List<Identifier>();
        ids.AddRange(Left.GetPropertyReferences());
        ids.AddRange(Right.GetPropertyReferences());
        return ids;
    }

    public override IEnumerable<Identifier> GetSettedReferences()
    {
        return Enumerable.Empty<Identifier>();
    }
}
