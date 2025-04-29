using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

[DebuggerDisplay("{DebugString}")]
public class NonTerminalStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    StackState<IN,OUT> Sibling { get; set; }
    
  public NonTerminalClause<IN,OUT> NonTerminal { get; set; }
    
    public List<SyntaxParseResult<IN,OUT>> Children { get; set; } = new List<SyntaxParseResult<IN,OUT>>();
    
    public int Index { get; set; }
    
    
    
    public override string DebugString => $"Non-Terminal {NonTerminal.NonTerminalName} [{Index}] @{Position}";
    
    public NonTerminalStackState(StackState<IN, OUT> parent, NonTerminalClause<IN, OUT> nonTerminal, StackState<IN,OUT> sibling = null) : base(parent)
    {
        NonTerminal = nonTerminal;
        Sibling = sibling;
        Index = 0;
        Type = StackStateType.NonTerminal;
    }

    public SyntaxParseResult<IN, OUT> LastResult => Children.Last();
    
    
    
    public NonTerminalStackState<IN, OUT> Shift()
    {
        var nextState = new NonTerminalStackState<IN, OUT>(Parent, NonTerminal)
        {
            Index = Index + 1,
            Tokens = Tokens,
            Position = Position,
        };
        return nextState;
    }
    
    public void AddChild(SyntaxParseResult<IN, OUT> result)
    {
        Children.Add(result);
    }

    public override string ToString()
    {
        return "non Terminal: " + NonTerminal.NonTerminalName;
    }
}