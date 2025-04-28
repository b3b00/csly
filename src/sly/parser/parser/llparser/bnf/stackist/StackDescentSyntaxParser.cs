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
    public Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }
    public SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, string startingNonTerminal = null)
    {
        var stack = new Stack<StackState<IN, OUT>>();
        var root = new StackState<IN, OUT>();
        var start = Configuration.StartingRule ?? startingNonTerminal;
        if (string.IsNullOrEmpty(start))
        {
            throw new Exception("No starting rule defined");
        }

        NonTerminalClause<IN, OUT> startNode = new NonTerminalClause<IN, OUT>(start);
        StackState<IN, OUT> state = new StackState<IN, OUT>(root, startNode)
        {
            Position = 0,
            Tokens = tokens
        };
        
        stack.Push(root);
        stack.Push(state);

        var current = stack.Pop();
        while (current != null && current.Type != StackStateType.Root)
        {
            Console.WriteLine(current.ToString() + " @" + current.Position+ " depth : "+stack.Count);
            if (current.Type == StackStateType.NonTerminal)
            {
                ParseNonTerminal(current, stack);
            }
            else if (current.Type == StackStateType.Terminal)
            {
                ParseTerminal(current, stack);
            }
            else if (current.Type == StackStateType.Rule)
            {
                // TODO
                ParseRule(current, stack);
            }

            var prev = current;
            current = stack.Pop();
            if (current == null || current.Type == StackStateType.Root)
            {
                ;
            }
        }
        
            Console.WriteLine("done");
        Console.WriteLine(current);
        return null; // TODO: return the result
    }

   

    public void Init(ParserConfiguration<IN, OUT> configuration, string root)
    {
        Configuration = configuration;
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

    public void ParseNonTerminal(StackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        NonTerminalClause<IN,OUT> nonTerminal = state.NonTerminal;
        if (Configuration.NonTerminals.TryGetValue(nonTerminal.NonTerminalName, out var nonTerminalClause))
        {
            var rules = nonTerminalClause.Rules;
            StackState<IN, OUT> sibling = null;
            for(int i = rules.Count-1; i >= 0; i--)
            {
                var rule = rules[i];
                
                var ruleState = new RuleStackState<IN, OUT>(state,rule, sibling) {
                    Tokens = state.Tokens,
                    Position = state.Position
                };
                sibling = ruleState;
                stack.Push(ruleState);
            }
        }
        else
        {
            throw new Exception($"Non terminal {nonTerminal.NonTerminalName} not found");
        }
    }
    
    public void ParseTerminal(StackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)

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

        var x = state.Parent.AddChild(result);
        if (x != null)
        {
            PopTill(stack, x);
        }
        
    }
    
    private void ParseRule(StackState<IN, OUT> current, Stack<StackState<IN, OUT>> stack)
    {
        var rule = current.Rule;
        if (rule != null)
        {
            for(int i = rule.Clauses.Count-1; i >= 0; i--)
            {
                var clause = rule.Clauses[i];
                StackState<IN, OUT> sibling = null;
                switch (clause)
                {
                    case TerminalClause<IN, OUT> terminalClause:
                    {
                        var terminalState = new TerminalStackState<IN, OUT>(current, terminalClause)
                        {
                            Tokens = current.Tokens,
                            Position = current.Position
                        };
                        stack.Push(terminalState);
                        break;
                    }
                    case NonTerminalClause<IN, OUT> nonTerminalClause:
                    {
                        var nonTerminalState = new NonTerminalStackState<IN, OUT>(current, nonTerminalClause, sibling)
                        {
                            Tokens = current.Tokens,
                            Position = current.Position
                        };
                        stack.Push(nonTerminalState);
                        break;
                    }
                }
                sibling = stack.Peek();
            }
        }
    }
    
    private void PopTill(Stack<StackState<IN, OUT>> stack, StackState<IN,OUT> state)
    {
        while (stack.Count > 0 && stack.Peek() != state)
        {
            stack.Pop();
        }
    }
    
}