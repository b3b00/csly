using System;
using System.Collections.Generic;
using sly.parser.llparser.bnf.stackist;
using sly.parser.syntax.grammar;

namespace sly.parser.parser.llparser.ebnf.stackist.state;

public class OptionStackState<IN,OUT> : EbnfStackState<IN,OUT> where IN : struct, Enum
{
    private readonly OptionClause<IN, OUT> _clause;
    private readonly StackState<IN, OUT> _parent;
    
    public override EbnfStackStateType EbnfStackType => EbnfStackStateType.Option;
    
    private List<SyntaxParseResult<IN,OUT>> _children;

  public List<SyntaxParseResult<IN,OUT>> Children => _children;

    public IClause<IN, OUT> OptionalClause => _clause.Clause;


    public OptionStackState(OptionClause<IN, OUT> option, StackState<IN, OUT> parent)
    {
        _clause =  option;
        Parent = parent;
        _children = new List<SyntaxParseResult<IN,OUT>>();
    } 
    
    public override void SetResult(SyntaxParseResult<IN, OUT> result)
    {
        base.SetResult(result);
        AddChild(result);
    }

    public bool IsOk => Result != null && Result.IsOk;
    
    
    public void AddChild(SyntaxParseResult<IN, OUT> result)
    {
        if (result.IsOk)
        {
            _children.Add(result);
        }
    }

}