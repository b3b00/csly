using System;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

public class StackState<IN, OUT> where IN : struct, Enum
{
    
    public virtual string DebugString
    {
        get
        {
            return "StackState : " + Type.ToString() + " : " + Position + " : " + (Parent != null ? Parent.Position.ToString() : "null");
        }
    }
    public int Position { get; set; }
    
    public int StartPosition { get; set; }
    
    public Token<IN> CurrentToken => Tokens[Position];
    
    public virtual StackStateType Type { get; }
    
    public Token<IN>[] Tokens { get; set; }
    
    public StackState<IN,OUT> Parent { get; set; }

    public SyntaxParseResult<IN, OUT> Result { get; protected set; } = null;

    public StackState(StackState<IN,OUT> parent)
    {
        Parent = parent;
    }

    public StackState(StackState<IN, OUT> parent, TerminalClause<IN, OUT> terminal)
    {
        Parent = parent;
        Type = StackStateType.Terminal;
    }
   

    public StackState()
    {
        Parent = null;
        Type = StackStateType.Root;
    }

    public virtual void SetResult(SyntaxParseResult<IN, OUT> result)
    {
        if (result == null)
        {
            ;
        }
        Result = result;
    }

    
}