using System;
using System.Collections.Generic;
using System.Linq;
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
    Root,
    Result
}

public partial class StackDescentSyntaxParser<IN, OUT> : ISyntaxParser<IN, OUT> where IN : struct, Enum
{
    public Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }

    public ParserConfiguration<IN, OUT> Configuration { get; set; }

    public string StartingNonTerminal { get; set; }

    public string I18n { get; set; }

    public SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, string startingNonTerminal = null)
    {
        var stack = new Stack<StackState<IN, OUT>>();
        var root = new RootStackState<IN, OUT>();
        var start = Configuration.StartingRule ?? startingNonTerminal;
        if (string.IsNullOrEmpty(start))
        {
            throw new Exception("No starting rule defined");
        }

        NonTerminalClause<IN, OUT> startNode = new NonTerminalClause<IN, OUT>(start);
        StackState<IN, OUT> state = new NonTerminalStackState<IN, OUT>(root, startNode)
        {
            Position = 0,
            Tokens = tokens
        };

        stack.Push(root);
        stack.Push(state);

        var current = stack.Pop();
        while (current != null)
        {
            Console.WriteLine(current.ToString() + " @" + current.Position + " depth : " + stack.Count);
            switch (current)
            {
                case RuleStackState<IN, OUT> ruleState:
                    ParseRule(ruleState, stack);
                    break;
                case NonTerminalStackState<IN, OUT> nonTerminalState:
                    ParseNonTerminal(nonTerminalState, stack);
                    break;
                case TerminalStackState<IN, OUT> terminalState:
                    ParseTerminal(terminalState, stack);
                    break;
                case RootStackState<IN, OUT> rootState:
                {
                    return rootState.Children.Last();
                }
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
        InitializeStartingTokens(Configuration, root ?? configuration.StartingRule);
    }

    public string Dump() => Configuration.Dump();



    public StackDescentSyntaxParser(string i18n,
        ParserConfiguration<IN, OUT> configuration)
    {
        Init(configuration, configuration.StartingRule);
    }

    public void ParseNonTerminal(NonTerminalStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        NonTerminalClause<IN, OUT> nonTerminal = state.NonTerminal;

        if (state.Index > 0 && state.LastResult.IsOk)
        {
            if (state.Parent is RuleStackState<IN, OUT> ruleState)
            {
                ruleState.AddChild(state.LastResult);
            }
            else if (state.Parent is RootStackState<IN, OUT> rootState)
            {
                rootState.AddChild(state.LastResult);
            }
            else
            {
                Console.WriteLine(
                    $"HOOPS something bad here ! nonterminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
            }

            return;
        }
        

        if (Configuration.NonTerminals.TryGetValue(nonTerminal.NonTerminalName, out var nonTerminalClause))
        {
            var rules = nonTerminalClause.Rules;
            if (state.Index >= rules.Count)
            {
                if (state.Parent is RuleStackState<IN, OUT> ruleState)
                {
                    ruleState.AddChild(state.LastResult);
                }
                else
                {
                    Console.WriteLine(
                        $"HOOPS something bad here ! nonterminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
                }

                return;
            }

            // first stack self shifting
            var nextState = state.Shift();
            stack.Push(nextState);

            var rule = rules[state.Index];
            if (rule.Match(state.Tokens, state.Position, Configuration)) // TODO beware the position ?
            {
                var ruleState = new RuleStackState<IN, OUT>(nextState, rule)
                {
                    Tokens = state.Tokens,
                    Position = state.Position
                };
                stack.Push(ruleState);
            }
        }
    }


    public void ParseTerminal(TerminalStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        var terminalState = state as TerminalStackState<IN, OUT>;

        TerminalClause<IN, OUT> terminal = terminalState.Terminal;
        var result = new SyntaxParseResult<IN, OUT>();
        result.IsError = !terminal.Check(terminalState.Tokens[terminalState.Position]);
        result.EndingPosition = !result.IsError ? terminalState.Position + 1 : terminalState.Position;
        var token = terminalState.Tokens[terminalState.Position];
        token.Discarded = terminal.Discarded;
        token.IsExplicit = terminal.IsExplicitToken;
        result.Root = new SyntaxLeaf<IN, OUT>(token, terminal.Discarded);
        result.HasByPassNodes = false;
        if (result.IsError)
        {
            result.AddError(new UnexpectedTokenSyntaxError<IN>(token, LexemeLabels, I18n, terminal.ExpectedToken));
        }

        if (state.Parent is RuleStackState<IN, OUT> parentState)
        {
            parentState.AddChild(result);
        }
        else
        {
            Console.WriteLine(
                $"HOOPS something bad here ! terminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
        }
        // TODO more ? I don't think so 

    }

    private void ParseRule(RuleStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        var rule = state.Rule;
        if (state.Index > 0 && state.IsEnded)
        {
            // TODO : rule has ended ...
            if (state.Parent is NonTerminalStackState<IN, OUT> parentState)
            {
                // TODO : get build the result
                var result = new SyntaxParseResult<IN, OUT>();
                var node = new SyntaxNode<IN, OUT>(state.Rule.NodeName, state.Children.Select(x => x.Root).ToList());// TODO
                node.Visitor = state.Rule.GetVisitorMethod();
                result.Root = node;
                parentState.AddChild(result);
                
            }
            else
            {
                Console.WriteLine(
                    $"HOOPS something very bad happened here ! terminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
            }
            return;
        }

        if (rule != null)
        {
            // new position is ending position of the last result
            int newPosition = state.Index > 0 ? state.LastResult.EndingPosition : state.Position;


            state.Position = newPosition;


            var clause = rule.Clauses[state.Index];
            // first self stack with index shift
            var nextRuleState = state.Shift();
            nextRuleState.Position = newPosition;

            stack.Push(nextRuleState);

            // then push the clause

            switch (clause)
            {
                case TerminalClause<IN, OUT> terminalClause:
                {
                    var terminalState = new TerminalStackState<IN, OUT>(nextRuleState, terminalClause)
                    {
                        Tokens = state.Tokens,
                        Position = newPosition
                    };
                    stack.Push(terminalState);
                    break;
                }
                case NonTerminalClause<IN, OUT> nonTerminalClause:
                {
                    var nonTerminalState = new NonTerminalStackState<IN, OUT>(nextRuleState, nonTerminalClause)
                    {
                        Tokens = state.Tokens,
                        Position = state.Position
                    };
                    stack.Push(nonTerminalState);
                    break;
                }
            }
        }
    }


    private void PopTill(Stack<StackState<IN, OUT>> stack, StackState<IN, OUT> state)
    {
        while (stack.Count > 0 && stack.Peek() != state)
        {
            stack.Pop();
        }
    }

}