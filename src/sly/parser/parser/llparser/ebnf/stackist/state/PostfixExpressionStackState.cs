using System;
using System.Diagnostics;
using sly.parser.generator;
using sly.parser.llparser.bnf.stackist;
using sly.parser.syntax.grammar;

namespace sly.parser.parser.llparser.ebnf.stackist.state;

[DebuggerDisplay("{DebugString}")]
public class PostfixExpressionStackState<IN,OUT> : EbnfStackState<IN,OUT> where IN : struct, Enum
{
    public override string DebugString => $"expression  {Rule.Dump()} [{ExpressionState}]  @{Position}";

    public SyntaxParseResult<IN, OUT> Left;
    public SyntaxParseResult<IN, OUT> Operator;
    public SyntaxParseResult<IN, OUT> Right;
    
    public override EbnfStackStateType EbnfStackType => EbnfStackStateType.Postfix;
    
    public ExpressionRuleState ExpressionState { get; set; }
    
    public Rule<IN, OUT> Rule { get; set; }

    private Affix Affix => Rule.ExpressionAffix;
    
    public PostfixExpressionStackState(Rule<IN, OUT> rule, StackState<IN, OUT> parent) : base(parent)
    {
        Rule = rule;
        ExpressionState = ExpressionRuleState.NotStarted;
    }
    
    public override string ToString()
    {
        return "Expresion: " + Rule.Dump();
    }
}