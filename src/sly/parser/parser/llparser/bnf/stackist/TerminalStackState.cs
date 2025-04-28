using System;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

public class TerminalStackState<In,OUT> : StackState<In,OUT> where In : struct, Enum
{
    public TerminalStackState(StackState<In, OUT> parent, TerminalClause<In, OUT> terminal) : base(parent, terminal)
    {
    }

    public override string ToString()
    {
        return "Terminal: " + Terminal.ExpectedToken.ToString();
    }
}