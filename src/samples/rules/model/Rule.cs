using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace rules.model;


public class Rule : IRule
{
    public Rule(int number, RuleExpression condition, Statements action, Position position)
    {
        
        Position = position;
        Number = number;
        Condition = condition;
        Action = action;
    }

    private List<RuleComment> _comments;

    public List<RuleComment> PrecedingComments
    {
        get => _comments;
        set
        {
            _comments = value;
        }
    }

    public List<RuleComment> ConditionPrecedingComments { get; set; }
    
    public Rule(Rule rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        Position = rule.Position;
        Number = rule.Number;
        Condition = rule.Condition;
        Action = rule.Action;
        PrecedingComments = rule.PrecedingComments;
        ConditionPrecedingComments = rule.ConditionPrecedingComments;
    }

    [JsonPropertyName("Number")]
    public int Number { get; }

    public RuleExpression Condition { get; }

    public Statements Action { get; }

    public Position Position { get; set; }

    [ExcludeFromCodeCoverage]
    public string Dump()
    {
        return $@"RULE #{Number}
IF {Condition.Dump()} THEN
    {Action.Dump()}
END IF

END RULE
";
    }

   
}
