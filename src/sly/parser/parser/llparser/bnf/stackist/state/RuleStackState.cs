using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

[DebuggerDisplay("{DebugString}")]
public class RuleStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    public override string DebugString => $"Rule {Rule.RuleString} [{Index}] @{Position}";
    
    public int Index { get; set; }
    
    public Rule<IN, OUT> Rule { get; set; }
    
    public bool IsEnded => Index >= Rule.Clauses.Count || LastResult.IsError;
    
    public SyntaxParseResult<IN, OUT> LastResult => Children.Last();
    
    public RuleStackState(StackState<IN, OUT> parent, Rule<IN, OUT> rule) : base(parent)
    {
        Rule = rule;
        Type = StackStateType.Rule;
        Index = 0;
    }

    public RuleStackState<IN, OUT> Shift()
    {
        var nextState = new RuleStackState<IN, OUT>(Parent, Rule)
        {
            Index = Index + 1,
            Tokens = Tokens,
            Position = Position,
        };
        return nextState;
    }
    
    public List<SyntaxParseResult<IN,OUT>> Children { get; set; } = new List<SyntaxParseResult<IN,OUT>>();

    public void AddChild(SyntaxParseResult<IN, OUT> result)
    {
        Children.Add(result);
    }
    
    
    
    public override string ToString()
    {
        return "Rule: " + Rule.RuleString;
    }
}