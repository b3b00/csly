using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

public static class RuleExt
{
    public static string Progress<IN, OUT>(this RuleStackState<IN, OUT> state) where IN : struct, Enum
    {
        StringBuilder b = new StringBuilder();
        b.Append("<<").Append(state.Id).Append(">> @").Append(state.Position).Append(" :: ");
        b.Append(state.Rule.NonTerminalName).Append(" : ");
        for (int i = 0; i < state.Rule.Clauses.Count; i++)
        {
            if (i == state.Index)
            {
                b.Append(">");
            }

            b.Append(state.Rule.Clauses[i].Dump());
            b.Append(" ");
        }

        if (state.Index >= state.Rule.Clauses.Count)
        {
            b.Append(" DONE :").Append(state.LastResult.IsError ? " KO" : " OK");
            
        } 
        return b.ToString();
    }
}


[DebuggerDisplay("{DebugString}")]
public class RuleStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    
    private static int Counter = 0;
    
    public int Id { get; set; }
    public override string DebugString => $"Rule <<{Id}>> {Rule.RuleString} [{Index}] @{Position}";
    
    public int Index { get; set; }
    
    public Rule<IN, OUT> Rule { get; set; }
    
    public bool IsEnded => Index >= Rule.Clauses.Count || LastResult == null || LastResult.IsError;
    
    public SyntaxParseResult<IN, OUT> LastResult => Children.Last(x => x != null);
    
    public RuleStackState(StackState<IN, OUT> parent, Rule<IN, OUT> rule) : base(parent)
    {
        Rule = rule;
        Type = StackStateType.Rule;
        Index = 0;
        Id = Counter++;
        Children = new List<SyntaxParseResult<IN, OUT>>(rule.Clauses.Count);
        for (int i = 0; i < rule.Clauses.Count; i++)
        {
            Children.Add(null);
        }
    }
  
    public List<SyntaxParseResult<IN,OUT>> Children { get; set; } = new ();

    public void AddChild(SyntaxParseResult<IN, OUT> result)
    {
        if (Id == 9)
        {
            ;
        }
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