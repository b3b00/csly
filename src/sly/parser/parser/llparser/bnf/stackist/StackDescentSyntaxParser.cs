using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.bnf.stackist;

public enum StackStateType
{
    Terminal,
    NonTerminal,
    Rule,
    Root
}

public class StackDescentSyntaxParser<IN, OUT>: ISyntaxParser<IN,OUT> where IN : struct, Enum
{
    
    public StackState<IN, OUT> CurrentState { get; set; }
    
    public Stack<StackState<IN, OUT>> Stack { get; set; } = new Stack<StackState<IN, OUT>>();
    
    public Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }
    public SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, string startingNonTerminal = null)
    {
        var root = new StackState<IN, OUT>();
        var start = Configuration.StartingRule ?? startingNonTerminal;
        if (string.IsNullOrEmpty(start))
        {
            throw new Exception("No starting rule defined");
        }

        NonTerminalClause<IN, OUT> startNode = new NonTerminalClause<IN, OUT>(start);
        StackState<IN,OUT> state = new StackState<IN, OUT>(root,startNode);
        Stack.Push(root);
        Stack.Push(state);

        var current = Stack.Pop();
        while (current != null && current.Type != StackStateType.Root)
        {
            if (current.Type == StackStateType.NonTerminal)
            {
                ParseNonTerminal(current);
            }
            else if (current.Type == StackStateType.Terminal)
            {
                ParseTerminal(current);
            }
            else if (current.Type == StackStateType.Rule)
            {
                // TODO
                var rule = current.Rule;
                var ruleState = new StackState<IN, OUT>(current.Parent, rule);
                ruleState.Parent = current.Parent;
                ruleState.Rule = rule;
                ruleState.NonTerminal = current.NonTerminal;
                ruleState.Tokens = current.Tokens;
                ruleState.Position = current.Position;
                Stack.Push(ruleState);
            }

            current = Stack.Pop();
        }
        
            
        
        throw new NotImplementedException();
    }

    public void Init(ParserConfiguration<IN, OUT> configuration, string root)
    {
        Configuration = configuration;
        CurrentState = new StackState<IN, OUT>();
        Stack = new Stack<StackState<IN, OUT>>();
        Stack.Push(CurrentState);
    }

    public string Dump() => Configuration.Dump();

    public ParserConfiguration<IN, OUT> Configuration { get; set; }
    
    public string StartingNonTerminal { get; set; }
        
    public string I18n { get; set; }

    public StackDescentSyntaxParser(string i18n,
        ParserConfiguration<IN, OUT> configuration)
    {
        Init(configuration,configuration.StartingRule);
    }

    public void ParseNonTerminal(StackState<IN, OUT> state)
    {
        NonTerminalClause<IN,OUT> nonTerminal = state.NonTerminal;
        if (Configuration.NonTerminals.TryGetValue(nonTerminal.NonTerminalName, out var nonTerminalClause))
        {
            var rules = nonTerminalClause.Rules;
            for(int i = rules.Count-1; i >= 0; i--)
            {
                var rule = rules[i];
                var ruleState = new StackState<IN, OUT>(state,rule);
                ruleState.Parent = state;
                ruleState.Rule = rule;
                ruleState.NonTerminal = nonTerminal;
                ruleState.Tokens = state.Tokens;
                ruleState.Position = state.Position;
                Stack.Push(ruleState);
            }
        }
        else
        {
            throw new Exception($"Non terminal {nonTerminal.NonTerminalName} not found");
        }
    }
    
    public void ParseTerminal(StackState<IN,OUT> state)

    {
        TerminalClause<IN,OUT> terminal = state.Terminal;
        var result = new SyntaxParseResult<IN, OUT>();
        result.IsError = !terminal.Check(state.Tokens[state.Position]);
        result.EndingPosition = !result.IsError ? state.Position + 1 : state.Position;
        var token = state.Tokens[state.Position];
        token.Discarded = terminal.Discarded;
        token.IsExplicit = terminal.IsExplicitToken;
        result.Root = new SyntaxLeaf<IN, OUT>(token, terminal.Discarded);
        result.HasByPassNodes = false;
        if (result.IsError)
        {
            result.AddError(new UnexpectedTokenSyntaxError<IN>(token, LexemeLabels, I18n, terminal.ExpectedToken));
        }

        state.Parent.AddChild(result);
        
    }
    
}