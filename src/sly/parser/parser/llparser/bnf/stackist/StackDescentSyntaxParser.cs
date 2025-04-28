using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

public enum StackStateType
{
    Terminal,
    NonTerminal,
    Rule,
    Root
}

public class StackState<IN, OUT> where IN : struct, Enum
{
    public int Position { get; set; }
    
    public Rule<IN, OUT> Rule { get; set; }
    
    public TerminalClause<IN,OUT> Terminal { get; set; }
    
    public NonTerminalClause<IN,OUT> NonTerminal { get; set; }
    
    public StackStateType Type { get; set; }
    
    public List<Token<IN>> Tokens { get; set; }
    
    public 

    public StackState(Rule<IN,OUT> rule)
    {
        Rule = rule;
        Type = StackStateType.Rule;
    }
    
    public StackState(TerminalClause<IN,OUT> terminal)
    {
        Terminal = terminal;
        Type = StackStateType.Terminal;
    }
    
    public StackState(NonTerminalClause<IN,OUT> nonTerminal)
    {
        NonTerminal = nonTerminal;
        Type = StackStateType.NonTerminal;
    }

    public StackState()
    {
        Type = StackStateType.Root;
    }
}

public class StackDescentSyntaxParser<IN, OUT> where IN : struct, Enum
{
    
    public StackState<IN, OUT> CurrentState { get; set; }

    public StackDescentSyntaxParser()
    {
        CurrentState = new StackState<IN, OUT>();
    }
    
    public 
    
}