using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

[DebuggerDisplay("{DebugString}")]
public class TerminalStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    public TerminalClause<IN,OUT> Terminal { get; set; }
    
    public override  StackStateType Type => StackStateType.Terminal;
    public TerminalStackState(StackState<IN, OUT> parent, TerminalClause<IN, OUT> terminal) : base(parent)
    {
        Terminal = terminal;
        
    }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return "Terminal: " + Terminal.ExpectedToken.ToString();
    }
}