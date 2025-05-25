using System;
using System.Diagnostics;
using sly.parser.generator;
using sly.parser.llparser.bnf.stackist;
using sly.parser.syntax.grammar;

namespace sly.parser.parser.llparser.ebnf.stackist.state;

[DebuggerDisplay("{DebugString}")]
public class PrefixExpressionStackState<IN,OUT> : StackState<IN,OUT> where IN : struct, Enum
{
    public override string DebugString => $"expression  {Rule.Dump()} [{ExpressionState}]  @{Position}";

    public SyntaxParseResult<IN, OUT> Left;
    public SyntaxParseResult<IN, OUT> Operator;
    public SyntaxParseResult<IN, OUT> Right;
    
    public ExpressionRuleState ExpressionState { get; set; }
    
    public Rule<IN, OUT> Rule { get; set; }

    private Affix Affix => Rule.ExpressionAffix;
    
    public PrefixExpressionStackState(Rule<IN, OUT> rule, StackState<IN, OUT> parent) : base(parent)
    {
        Rule = rule;
        Type = StackStateType.Rule;
        ExpressionState = ExpressionRuleState.NotStarted;
    }
    
    public override string ToString()
    {
        return "Expresion: " + Rule.Dump();
    }
}