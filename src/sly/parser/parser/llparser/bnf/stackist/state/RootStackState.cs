using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace sly.parser.llparser.bnf.stackist;

[DebuggerDisplay("root")]
public class RootStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    
    public List<SyntaxParseResult<IN,OUT>> Children { get; set; } 

    public RootStackState() : base()
    {
        Children = new List<SyntaxParseResult<IN, OUT>>();
        Type = StackStateType.Root;
    }
    
    public void AddChild(SyntaxParseResult<IN, OUT> result)
    {
        Children.Add(result);
    }
    
    
}