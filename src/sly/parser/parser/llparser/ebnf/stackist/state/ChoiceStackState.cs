using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using sly.parser.llparser.bnf.stackist;
using sly.parser.syntax.grammar;

namespace sly.parser.parser.llparser.ebnf.stackist.state;

[DebuggerDisplay("{DebugString}")]
public class ChoiceStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    public override string DebugString => $"Choice  {Choice.Dump()} [{Index}]  @{Position}";
    
    public int Index { get; set; }
    
    public ChoiceClause<IN, OUT> Choice { get; set; }
    
    public ChoiceStackState(ChoiceClause<IN, OUT> choice, StackState<IN, OUT> parent) : base(parent)
    {
        Choice = choice;
        Type = StackStateType.Rule;
        Index = 0;
    }
    
    public override string ToString()
    {
        return "Choice: " + Choice.Dump();
    }
}