using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;
using System.IO;

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

    private const bool DEBUG = false;
    public Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }

    public ParserConfiguration<IN, OUT> Configuration { get; set; }

    public string StartingNonTerminal { get; set; }
    
    public string I18n { get; set; }


    public StackDescentSyntaxParser()
    {
        
    }
    public StackDescentSyntaxParser(string i18n,
        ParserConfiguration<IN, OUT> configuration)
    {
        I18n = i18n;
        Init(configuration, configuration.StartingRule);
    }
    
    public void Init(ParserConfiguration<IN, OUT> configuration, string root)
    {
        Configuration = configuration;
        RecursiveDescentSyntaxParser<IN, OUT> recursive =
            new RecursiveDescentSyntaxParser<IN, OUT>(configuration, configuration.StartingRule, I18n);
        recursive.Init(configuration, configuration.StartingRule);
    }

    public string Dump() => Configuration.Dump();


    public virtual void ParseExtension(StackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        
    }
    
    public SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, string startingNonTerminal = null)
    {
        
        
        var stack = new Stack<StackState<IN, OUT>>();
        var root = new RootStackState<IN, OUT>();
        var start =  startingNonTerminal ?? Configuration.StartingRule;
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
        var toks = string.Join(" ", tokens.Select(x => x.Value));
        Log($"start :: {toks}",stack);

        stack.Push(root);
        stack.Push(state);

        var current = stack.Pop();
        while (current != null)
        {
            
            switch (current)
            {
                case RuleStackState<IN, OUT> ruleState:
                    Log(ruleState.Progress(), stack);
                    ParseRule(ruleState, stack);
                    break;
                case NonTerminalStackState<IN, OUT> nonTerminalState:
                    Log(nonTerminalState.Progress(Configuration), stack);
                    ParseNonTerminal(nonTerminalState, stack);
                    break;
                case TerminalStackState<IN, OUT> terminalState:
                    // Log(current.DebugString, stack);
                    ParseTerminal(terminalState, stack);
                    break;
                case RootStackState<IN, OUT> rootState:
                {
                    return rootState.Result;
                }
                default:
                {
                    ParseExtension(current, stack);
                    break;
                }
            }
            current = stack.Pop();
        }

        return null;
    }

    private void ParseNonTerminal(NonTerminalStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        NonTerminalClause<IN, OUT> nonTerminal = state.NonTerminal;
        if (Configuration.NonTerminals.TryGetValue(nonTerminal.NonTerminalName, out var nonTerminalClause))
        {
            if (state.Index >= nonTerminalClause.Rules.Count && state.Result != null)
            {
                var realResult = state.Result;
                // success may have happened previously !
                if (realResult.IsError && state.Successes.Any(x => x.IsOk))
                {
                    Log(state.DebugString+$" ended with {state.Successes.Count} successes",stack,1);
                    realResult = state.Successes.OrderBy(x => x.EndingPosition).Last();
                } 
                else if (state.Successes.Count(x => x.IsOk) > 1)
                {
                    Log(state.DebugString+$" ended with {state.Successes.Count} successes",stack,1);
                    realResult = state.Successes.OrderBy(x => x.EndingPosition).Last();
                    Log($" choosing one ending at {realResult.EndingPosition}",stack,2);
                }
                if (state.Parent is RuleStackState<IN, OUT> ruleState)
                {
                    ruleState.SetResult(realResult);
                }
                else if (state.Parent is RootStackState<IN, OUT> rootState)
                {
                    rootState.SetResult(realResult);
                }
                else
                {
                    throw new Exception($"HOOPS something bad here ! nonterminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
                }
                return;
            }

            if (state.Index >= nonTerminalClause.Rules.Count && state.Result == null)
            {
                var result = new SyntaxParseResult<IN, OUT>();
                result.IsError = true;
                result.EndingPosition = state.Position;
                var expected = nonTerminalClause.Rules.SelectMany(x => x.PossibleLeadingTokens).Distinct().ToArray();

                if (state.Successes.Any())
                {
                    // Log($"{state.NonTerminal.NonTerminalName}<<{state.Index}>> : no more alternative but at least one match has been found ",stack,1);
                    var last = state.Successes.OrderBy(x => x.EndingPosition).Last();
                    if (state.Parent is RuleStackState<IN, OUT> rState)
                    {
                        rState.SetResult(result);
                    }
                    else if (state.Parent is RootStackState<IN, OUT> rootState)
                    {
                        rootState.SetResult(result);
                    }
                    else
                    {
                        throw new Exception(
                            $"HOOPS something bad here ! nonterminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
                    }

                    return;
                }
                
                if (state.Position >= state.Tokens.Length)
                {
                    var error = new UnexpectedTokenSyntaxError<IN>(state.Tokens.Last(), LexemeLabels, I18n,
                        expected);
                    result.AddError(error);
                }
                else
                {
                    var error = new UnexpectedTokenSyntaxError<IN>(state.Tokens[state.Position], LexemeLabels, I18n,
                        expected);
                    result.AddError(error);
                }

                if (state.Parent is RuleStackState<IN, OUT> ruleState)
                {
                    ruleState.SetResult(result);
                }
                else if (state.Parent is RootStackState<IN, OUT> rootState)
                {
                    rootState.SetResult(result);
                }
                else
                {
                    throw new Exception(
                        $"HOOPS something bad here ! nonterminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
                }
                return;
            }

            if (state.Index > 0 && state.Result != null && state.Result.IsOk)
            {
                // here : last rule returned OK => returning right now
                if (state.Parent is RuleStackState<IN, OUT> ruleState)
                {
                    ruleState.SetResult(state.Result);
                }
                else if (state.Parent is RootStackState<IN, OUT> rootState)
                {
                    rootState.SetResult(state.Result);
                }
                else
                {
                    throw new Exception(
                        $"HOOPS something bad here ! nonterminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
                }

                bool hasParseEnded = state.Result.EndingPosition  >= state.Tokens.Length-1;
                // other rules may beter match if  parse has not ended
                if (hasParseEnded)
                {
                    return;
                }
                else
                {
                    Log("end of token stream not reached, looking forward", stack, 1);
                }

                //return;
            }



            var rules = nonTerminalClause.Rules;
            if (state.Index >= rules.Count)
            {
                if (state.Parent is RuleStackState<IN, OUT> ruleState)
                {
                    ruleState.SetResult(state.Result);
                }
                else if (state.Parent is RootStackState<IN, OUT> rootState)
                {
                    rootState.SetResult(state.Result);
                }
                else
                {
                    throw new Exception(
                        $"HOOPS something bad here ! nonterminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
                }

                return;
            }

            // first stack self shifting
            var rule = rules[state.Index];
            state.Index++;
            stack.Push(state);

            
            if (state.Position >= state.Tokens.Length)
            {
                return; 
            }

            if (rule.Match(state.Tokens, state.Position, Configuration))
            {
                var ruleState = new RuleStackState<IN, OUT>(state, rule)
                {
                    Tokens = state.Tokens,
                    Position = state.Position
                };
                stack.Push(ruleState);
            }
            else
            {
                Log($"KO rule (( {rule.RuleString} )) does not match {state.Tokens[state.Position]}",stack,1);
                var result = new SyntaxParseResult<IN, OUT>();
                var token = state.Tokens[state.Position];

                result.IsError = true;
                var expected = rule.PossibleLeadingTokens;
                result.EndingPosition = state.Position;

                result.AddError(new UnexpectedTokenSyntaxError<IN>(token, LexemeLabels, I18n, expected.ToArray()));
                if (state.Parent is RuleStackState<IN, OUT> ruleState)
                {
                    ruleState.SetResult(result);
                }
                else if (state.Parent is RootStackState<IN, OUT> rootState)
                {
                    rootState.SetResult(result);
                }
                else
                {
                    throw new Exception(
                        $"HOOPS something bad here ! nonterminal's parent should not be a {state.Parent.GetType().Name} : {state.Parent}");
                }
            }
        }
        else
        {
            throw new Exception($"ERRRRRROR  >{state.NonTerminal.NonTerminalName}< not found");
        }
    }


    public void ParseTerminal(TerminalStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        var terminalState = state as TerminalStackState<IN, OUT>;
        TerminalClause<IN, OUT> terminal = terminalState.Terminal;
        var result = new SyntaxParseResult<IN, OUT>();
        var token = terminalState.Tokens[terminalState.Position];
        var isError = !terminal.Check(token);
        result.IsError = isError;
        result.EndingPosition = !result.IsError ? terminalState.Position + 1 : terminalState.Position;
        if (isError)
        {
            Log($"error found {token} expected {terminal.ExpectedToken}",stack,1);
            result.AddError(new UnexpectedTokenSyntaxError<IN>(token, LexemeLabels, I18n, terminal.ExpectedToken));
            ;
        }
        else
        {
            Log($"OK {token}",stack,1);
        }
        
        token.Discarded = terminal.Discarded;
        token.IsExplicit = terminal.IsExplicitToken;
        result.Root = new SyntaxLeaf<IN, OUT>(token, terminal.Discarded);
        result.HasByPassNodes = false;
        if (result.IsError)
        {
            result.AddError(new UnexpectedTokenSyntaxError<IN>(token, LexemeLabels, I18n, terminal.ExpectedToken));
        }

        
        state.Parent.SetResult(result);
    }

    private void ParseRule(RuleStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        var rule = state.Rule;
        
        if (state.Index > 0 && state.IsEnded)
        {
            if (state.LastResult.IsError)
            {
                Log("KO "+state.LastResult.GetErrors().First().ErrorMessage,stack,1);
            }
            else
            {
                Log("OK Rule",stack,1);
            }
            
            if (state.Parent is NonTerminalStackState<IN, OUT> parentState)
            {
                var result = new SyntaxParseResult<IN, OUT>();

                if (state.LastResult.IsError)
                {
                    if (state.LastResult == null)
                    {
                        ;
                    }
                    parentState.SetResult(state.LastResult);   
                }
                else
                {
                    string name = "";
                    if (!string.IsNullOrEmpty(state.Rule.NodeName))
                    {
                        name = state.Rule.NodeName;
                    }
                    else
                    {
                        name = state.Rule.NonTerminalName;
                    }
                    var node = new SyntaxNode<IN, OUT>(name,
                        state.Children.Select(x => x.Root).ToList()); 
                    node.Visitor = state.Rule.GetVisitorMethod();
                    node.LambdaVisitor = state.Rule.getLambdaVisitor(null);
                    result.Root = node;
                    // send new position upward
                    result.EndingPosition = state.Children.Last().EndingPosition;
                    parentState.SetResult(result);
                }
            }
            else
            {
                throw new Exception(
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
            state.Index++;
            state.Position = newPosition;

            stack.Push(state);

            // then push the clause

            
            PushClause(clause, stack, state);
        }
    }


    public void PushClause(IClause<IN, OUT> clause, Stack<StackState<IN, OUT>> stack, StackState<IN,OUT> parent)
    {
        switch (clause)
        {
            case TerminalClause<IN, OUT> terminalClause:
            {
                var terminalState = new TerminalStackState<IN, OUT>(parent, terminalClause)
                {
                    Tokens = parent.Tokens,
                    Position = parent.Position
                };
                stack.Push(terminalState);
                break;
            }
            case NonTerminalClause<IN, OUT> nonTerminalClause:
            {
                var nonTerminalState = new NonTerminalStackState<IN, OUT>(parent, nonTerminalClause)
                {
                    Tokens = parent.Tokens,
                    Position = parent.Position
                };
                stack.Push(nonTerminalState);
                break;
            }
            default:
            {
                PushClauseExtension(clause, stack, parent);
                break;
            }
            
        }
    }

    public virtual void PushClauseExtension(IClause<IN, OUT> clause, Stack<StackState<IN, OUT>> stack, StackState<IN,OUT> parent)
    {
        
    }

    private void Log(string message, Stack<StackState<IN, OUT>> stack, int plus = 0)
    {
        if (DEBUG)
        {
            string tab = "  ";
            for (int i = 0; i < stack.Count + plus; i++)
            {
                tab += "  ";
            }

            Console.WriteLine(tab + message);
        }
    }

}