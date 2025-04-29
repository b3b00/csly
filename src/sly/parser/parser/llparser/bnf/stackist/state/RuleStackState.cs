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
  
    public List<SyntaxParseResult<IN,OUT>> Children { get; set; } = new ();

    public void AddChild(SyntaxParseResult<IN, OUT> result)
    {
        if (result == null)
        {
            return;
        }
        Children.Add(result);
    }
    
    
    
    public override string ToString()
    {
        return "Rule: " + Rule.RuleString;
    }
}