using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

public static class NonterminalExt
{
    public static string Progress<IN, OUT>(this NonTerminalStackState<IN, OUT> state, ParserConfiguration<IN,OUT> config) where IN : struct, Enum
    {
        string count = "unknown";
        string rule = "";
        if (config.NonTerminals.TryGetValue(state.NonTerminal.NonTerminalName, out var nonTerminalClause))
        {
            count = nonTerminalClause.Rules.Count().ToString();
            if (state.Index >= 0 && state.Index < nonTerminalClause.Rules.Count)
            {
                rule = nonTerminalClause.Rules[state.Index].RuleString;
            }

        }

        return $"Non-Terminal<<{state.Id}>> {state.NonTerminal.NonTerminalName} [{state.Index}/{count} : {rule}] @{state.Position}";
    }
}


[DebuggerDisplay("{DebugString}")]
public class NonTerminalStackState<IN, OUT> : StackState<IN, OUT> where IN : struct, Enum
{

    public static int Counter = 0;

    public int Id { get; set; }

    StackState<IN, OUT> Sibling { get; set; }

    public NonTerminalClause<IN, OUT> NonTerminal { get; set; }

    public int Index { get; set; }
    
    public List<SyntaxParseResult<IN,OUT>> Successes  { get; set; }



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
        Successes = new List<SyntaxParseResult<IN,OUT>>();
    }

    public void AddSuccess(SyntaxParseResult<IN, OUT> success)
    {
        Successes.Add(success);
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