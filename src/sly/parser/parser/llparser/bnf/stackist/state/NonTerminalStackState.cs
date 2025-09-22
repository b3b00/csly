using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;


[DebuggerDisplay("{DebugString}")]
public class NonTerminalStackState<IN, OUT> : StackState<IN, OUT> where IN : struct, Enum
{
    public NonTerminalClause<IN, OUT> NonTerminal { get; set; }

    public int Index { get; set; }
    
    public List<SyntaxParseResult<IN,OUT>> Successes  { get; set; }
    
    public List<SyntaxParseResult<IN,OUT>> Errors  { get; set; }
    
    public override  StackStateType Type => StackStateType.NonTerminal;
    
    public bool IsError => Result != null && Result.IsError;

    public NonTerminalStackState(StackState<IN, OUT> parent, NonTerminalClause<IN, OUT> nonTerminal,
        StackState<IN, OUT> sibling = null) : base(parent)
    {
        NonTerminal = nonTerminal;
        Index = 0;
        Successes = new List<SyntaxParseResult<IN,OUT>>();
        Errors = new List<SyntaxParseResult<IN,OUT>>();
    }

    public void AddSuccess(SyntaxParseResult<IN, OUT> success)
    {
        Successes.Add(success);
    }
    
    public void AddError(SyntaxParseResult<IN, OUT> success)
    {
        Errors.Add(success);
    }

    public void SetResult(SyntaxParseResult<IN, OUT> result)
    {
        Result = result;
        if (result.IsOk)
        {
            AddSuccess(result);
        }
        else
        {
            AddError(result);
        }
    }

    public override string ToString()
    {
        return "non Terminal: " + NonTerminal.NonTerminalName;
    }
}