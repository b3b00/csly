using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace rules.model;

public class IfStatement : IStatement
{
    public IfStatement(RuleExpression condition, Statements thenBlock, Statements elseBlock, Position position)
    {
        Position = position;
        ThenBlock = thenBlock;
        ElseBlock = elseBlock;
        Condition = condition;
    }

    public List<RuleComment> PrecedingComments { get; set; }

    public Identifier Property { get; }

    public RuleExpression Condition { get; }

    public Statements ThenBlock { get; }

    public Statements ElseBlock { get; }

    public Position Position { get; set; }

    [ExcludeFromCodeCoverage]
    public string Dump()
    {
        var dumper = new StringBuilder();
        dumper.Append("IF ").AppendLine(Condition.Dump())
            .AppendLine("THEN")
            .AppendLine(ThenBlock.Dump());
        if (ElseBlock != null)
        {
            dumper.AppendLine("ELSE");
            dumper.AppendLine(ElseBlock.Dump());
        }

        dumper.AppendLine("END IF");

        return dumper.ToString();
    }

}
