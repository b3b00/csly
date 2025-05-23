using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser.llparser.bnf.stackist;
using sly.parser.syntax.grammar;

namespace sly.parser.parser.llparser.ebnf.stackist.state;

public class OneOrMoreStackState<IN, OUT>: StackState<IN,OUT> where IN : struct, Enum
{
    private readonly OneOrMoreClause<IN, OUT> _clause;
    private readonly StackState<IN, OUT> _parent;

    public int Index { get; set; }

    private List<SyntaxParseResult<IN,OUT>> _children;

    public List<SyntaxParseResult<IN, OUT>> Children => _children;

    public IClause<IN, OUT> RepeatedClause => _clause.Clause;

    public OneOrMoreStackState(OneOrMoreClause<IN, OUT> oneOrMore, StackState<IN, OUT> parent) 
    {
        _clause =  oneOrMore;
        Parent = parent;
        _children = new List<SyntaxParseResult<IN,OUT>>();
    } 
    
    public bool IsOk => Result != null && Result.IsOk;
    
    
    public override void SetResult(SyntaxParseResult<IN, OUT> result)
    {
        base.SetResult(result);
        if (result.IsOk)
        {
            AddChild(result);
        }
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


}