using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;


[DebuggerDisplay("{DebugString}")]
public class RuleStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    
    private static int Counter = 0;
    
    public int Id { get; set; }
    
    public int Index { get; set; }

    public override StackStateType Type => StackStateType.Rule;
    public Rule<IN, OUT> Rule { get; set; }
    
    public bool IsEnded => Index >= Rule.Clauses.Count || LastResult == null || LastResult.IsError;
    
    public SyntaxParseResult<IN, OUT> LastResult => Children.Last(x => x != null);
    
    public RuleStackState(StackState<IN, OUT> parent, Rule<IN, OUT> rule) : base(parent)
    {
        Rule = rule;
        Index = 0;
        Id = Counter++;
        Children = new List<SyntaxParseResult<IN, OUT>>(rule.Clauses.Count);
        for (int i = 0; i < rule.Clauses.Count; i++)
        {
            Children.Add(null);
        }
    }
  
    public List<SyntaxParseResult<IN,OUT>> Children { get; set; } = new ();

    public override void SetResult(SyntaxParseResult<IN, OUT> result)
    {
        base.SetResult(result);
        AddChild(result);
    }

    public void AddChild(SyntaxParseResult<IN, OUT> result)
    {
        if (Children.Any(x => x == null))
        {
            ;
        } 
        if (result == null)
        {
            return;
        }

        try
        {
            Children[Index-1] = result;
        }
        catch (Exception e)
        {
            ;
        }

        //Children.Add(result);
    }
    
    
    
    public override string ToString()
    {
        return "Rule: " + Rule.RuleString;
    }
}