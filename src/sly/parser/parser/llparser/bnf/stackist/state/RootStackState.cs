using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace sly.parser.llparser.bnf.stackist;

[DebuggerDisplay("root")]
public class RootStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    public override  StackStateType Type => StackStateType.Root;

    
}