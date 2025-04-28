using System;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

public class NonTerminalStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    StackState<IN,OUT> Sibling { get; set; }
    
    public NonTerminalStackState(StackState<IN, OUT> parent, NonTerminalClause<IN, OUT> nonTerminal, StackState<IN,OUT> sibling = null) : base(parent, nonTerminal)
    {
        Sibling = sibling;
    }


    public override StackState<IN, OUT> AddChild(SyntaxParseResult<IN, OUT> result)
    {
        if (result == null || result.IsError)
        {
            return Parent.AddChild(result);
        }
        
        //TODO : 
        /*
         * if ok
         *     if has sibling => pop to sibling and set sibling position according to result
         *     else => send result to parent (rule)
         * if ko :
         *     send result to parent (rule) : parent may trackback
         *
         *
         * 
         */
        return base.AddChild(result);
    }

    public override string ToString()
    {
        return "non Terminal: " + NonTerminal.NonTerminalName;
    }
}