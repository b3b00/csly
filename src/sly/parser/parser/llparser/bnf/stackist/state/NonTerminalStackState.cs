using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

[DebuggerDisplay("{DebugString}")]
public class NonTerminalStackState<IN, OUT> : StackState<IN, OUT> where IN : struct, Enum
{

    public static int Counter = 0;

    public int Id { get; set; }

    StackState<IN, OUT> Sibling { get; set; }

    public NonTerminalClause<IN, OUT> NonTerminal { get; set; }

    public int Index { get; set; }



    public override string DebugString => $"Non-Terminal<<{Id}>> {NonTerminal.NonTerminalName} [{Index}] @{Position}";
    public bool IsError => Result != null && Result.IsError;

    public NonTerminalStackState(StackState<IN, OUT> parent, NonTerminalClause<IN, OUT> nonTerminal,
        StackState<IN, OUT> sibling = null) : base(parent)
    {
        Id = Counter++;
        NonTerminal = nonTerminal;
        Sibling = sibling;
        Index = 0;
        Type = StackStateType.NonTerminal;
    }

    public void SetResult(SyntaxParseResult<IN, OUT> result)
    {
        Result = result;
    }

    public override string ToString()
    {
        return "non Terminal: " + NonTerminal.NonTerminalName;
    }
}