using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf.stackist;

public class StackState<IN, OUT> where IN : struct, Enum
{
    public int Position { get; set; }
    
    public Rule<IN, OUT> Rule { get; set; }
    
    public TerminalClause<IN,OUT> Terminal { get; set; }
    
    public NonTerminalClause<IN,OUT> NonTerminal { get; set; }
    
    public StackStateType Type { get; set; }
    
    public Token<IN>[] Tokens { get; set; }
    
    public StackState<IN,OUT> Parent { get; set; }
    
    public SyntaxParseResult<IN,OUT> Result { get; set; }

    public StackState(StackState<IN,OUT> parent, Rule<IN,OUT> rule)
    {
        Parent = parent;
        Rule = rule;
        Type = StackStateType.Rule;
    }

    public StackState(StackState<IN, OUT> parent, TerminalClause<IN, OUT> terminal)
    {
        Parent = parent;
        Terminal = terminal;
        Type = StackStateType.Terminal;
    }

    public StackState(StackState<IN,OUT> parent, NonTerminalClause<IN,OUT> nonTerminal)
    {
        Parent = parent;
        NonTerminal = nonTerminal;
        Type = StackStateType.NonTerminal;
    }

    public StackState()
    {
        Parent = null;
        Type = StackStateType.Root;
    }

    public List<SyntaxParseResult<IN,OUT>> Children { get; set; } = new List<SyntaxParseResult<IN,OUT>>();
    
    public virtual StackState<IN,OUT> AddChild(SyntaxParseResult<IN,OUT> result)
    {
        Children.Add(result);
        return null;
    }

    public Token<IN> GetToken()
    {
        return Tokens[Position];
    }
}