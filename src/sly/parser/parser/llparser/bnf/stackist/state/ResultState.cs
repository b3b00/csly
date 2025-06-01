using System;

namespace sly.parser.llparser.bnf.stackist;

public class ResultState<IN, OUT> : StackState<IN, OUT> where IN : struct, Enum
{
    public override  StackStateType Type => StackStateType.Result;
    public ResultState(StackState<IN, OUT> parent, SyntaxParseResult<IN,OUT> result) : base(parent) 
    {
        Result = result;
        
    }
    
}