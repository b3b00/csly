using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using sly.parser.llparser.bnf.stackist;
using sly.parser.syntax.grammar;

namespace sly.parser.parser.llparser.ebnf.stackist.state;


public abstract class EbnfStackState<IN, OUT> : StackState<IN, OUT> where IN : struct, Enum
{
    public virtual EbnfStackStateType EbnfStackType => EbnfStackStateType.None;

    public EbnfStackState()
    {
        
    }
    
    public EbnfStackState(StackState<IN, OUT> parent) : base(parent)
    {
        
    }

    public override StackStateType Type => StackStateType.Extension;
}

[DebuggerDisplay("{DebugString}")]
public class ChoiceStackState<IN,OUT> : EbnfStackState<IN,OUT> where IN : struct, Enum
{

    public override EbnfStackStateType EbnfStackType => EbnfStackStateType.Choice;
    
    private List<SyntaxParseResult<IN,OUT>> _children;

    public List<SyntaxParseResult<IN, OUT>> Children => _children;
    
    public int Index { get; set; }
    
    public ChoiceClause<IN, OUT> Choice { get; set; }
    
    public ChoiceStackState(ChoiceClause<IN, OUT> choice, StackState<IN, OUT> parent) : base(parent)
    {
        Choice = choice;
        Index = 0;
        _children = new List<SyntaxParseResult<IN,OUT>>();
    }
    
    public override void SetResult(SyntaxParseResult<IN, OUT> result)
    {
        base.SetResult(result);
        // if (result.IsOk)
        // {
            AddChild(result);
        // }
    }

    
    public void AddChild(SyntaxParseResult<IN, OUT> result)
    {
        if (result.IsOk)
            if (Children.Any(x => x == null))
            {
                _children.Add(result);
                ;
            } 
        if (result == null)
        {
            return;
        }

        try
        {
            if (Index - 1 <= Children.Count - 1)
            {
                var previous = Children[Index - 1];
                if (previous != null && previous.EndingPosition < result.EndingPosition)
                {
                    Children[Index - 1] = result;
                }
            }
            else
            {
                Children.Add(result);
            }
        }
        catch (Exception e)
        {
            ;
        }

        //Children.Add(result);
    }
    
    public override string ToString()
    {
        return "Choice: " + Choice.Dump();
    }
}