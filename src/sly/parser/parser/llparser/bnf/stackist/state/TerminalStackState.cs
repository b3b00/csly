using System;
using System.Diagnostics;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

[DebuggerDisplay("{DebugString}")]
public class TerminalStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    public TerminalClause<IN,OUT> Terminal { get; set; }
    
    public override string DebugString => $"Terminal {Terminal.ExpectedToken} @{Position}";
    
    public StackState<IN,OUT> Sibling { get; set; }
    
    public override  StackStateType Type => StackStateType.Terminal;
    public TerminalStackState(StackState<IN, OUT> parent, TerminalClause<IN, OUT> terminal) : base(parent)
    {
        Terminal = terminal;
        
    }

    public override string ToString()
    {
        return "Terminal: " + Terminal.ExpectedToken.ToString();
    }
}