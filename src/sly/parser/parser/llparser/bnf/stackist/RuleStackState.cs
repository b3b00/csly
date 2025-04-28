using System;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

public class RuleStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    StackState<IN,OUT> Sibling { get; set; }
    
    public RuleStackState(StackState<IN, OUT> parent, Rule<IN, OUT> rule, StackState<IN,OUT> sibling = null) : base(parent, rule)
    {
        Sibling = sibling;
    }

    public override StackState<IN, OUT> AddChild(SyntaxParseResult<IN, OUT> result)
    {
        base.AddChild(result);
        if (result == null || result.IsError)
        {
            if (Sibling != null)
            {
                return Sibling;
            }
            else
            {
                return Parent.AddChild(result);
            }
        }
        else
        {
            // rule is still ok => continue parsing
            return null;
        }
    }
    
    public override string ToString()
    {
        return "Rule: " + Rule.RuleString;
    }
}