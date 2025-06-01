using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.llparser.bnf;
using sly.parser.llparser.bnf.stackist;
using sly.parser.llparser.ebnf;
using sly.parser.parser.llparser.ebnf.stackist.state;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.parser.llparser.ebnf.stackist;

public enum EbnfStackStateType
{
    Infix,
    Prefix,
    Postfix,
    OneOrMore,
    ZeroOrMore,
    Choice,
    Option,
    None,
}

public class EBNFStackDescentSyntaxParser<IN, OUT> : StackDescentSyntaxParser<IN, OUT> where IN : struct, Enum
{
    public EBNFStackDescentSyntaxParser(string i18n, ParserConfiguration<IN, OUT> configuration)
    {
        I18n = i18n;
        Configuration = configuration;
    }

    public override void Init(ParserConfiguration<IN, OUT> configuration, string root)
    {
        Configuration = configuration;
        EBNFRecursiveDescentSyntaxParser<IN, OUT> recursive =
            new EBNFRecursiveDescentSyntaxParser<IN, OUT>(configuration, configuration.StartingRule, I18n);
        recursive.Init(configuration, configuration.StartingRule);
    }

    public override void ParseExtension(StackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        var ebnfState = state as EbnfStackState<IN, OUT>;
        
        switch (ebnfState.EbnfStackType)
        {
            case EbnfStackStateType.Infix:
            {
                ParseInfixExpressionRule(ebnfState as InfixExpressionStackState<IN, OUT>, stack);
                break;
            }
            case EbnfStackStateType.ZeroOrMore:
            {
                ParseZeroOrMore(ebnfState as ZeroOrMoreStackState<IN, OUT>, stack);
                break;
            }
            case EbnfStackStateType.OneOrMore:
            {
                ParseOneOrMore(ebnfState as OneOrMoreStackState<IN, OUT>, stack);
                break;
            }
            case EbnfStackStateType.Option:
            {
                ParseOption(ebnfState as OptionStackState<IN, OUT>, stack);
                break;
            }
            case EbnfStackStateType.Choice:
            {
                ParseChoice(ebnfState as ChoiceStackState<IN, OUT>, stack);
                break;
            }
            default : 
            {
                ;
                break;
            }
        }
    }

    private void ParseOption(OptionStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        if (state.Result == null)
        {
            // push itself
            stack.Push(state);

            PushClause(state.OptionalClause, stack, state);
        }
        else
        {
            var innerResult = state.Result;
            var result = new SyntaxParseResult<IN, OUT>();
            result.IsError = false;
            result.EndingPosition = innerResult.EndingPosition;
            if (state.Result.IsOk)
            {
                var node = new OptionSyntaxNode<IN, OUT>($"{state.OptionalClause.ToString()}?",
                    new List<ISyntaxNode<IN, OUT>>() { innerResult.Root }, null);
                result.Root = node;
                node.IsGroupOption = state.OptionalClause is NonTerminalClause<IN, OUT> nt && nt.IsGroup;


                var children = new List<ISyntaxNode<IN, OUT>> { innerResult.Root };
                result.Root =
                    new OptionSyntaxNode<IN, OUT>("nevermind", children, null);
                result.EndingPosition = innerResult.EndingPosition;
                result.HasByPassNodes = innerResult.HasByPassNodes;
                state.Parent.SetResult(result);
            }
            else if (state.Result.IsError)
            {
                if (state.OptionalClause is TerminalClause<IN, OUT>)
                {
                    result.Root = new SyntaxLeaf<IN, OUT>(Token<IN>.Empty(), false);
                    state.Parent.SetResult(result);
                }
                else if (state.OptionalClause is NonTerminalClause<IN, OUT> nonTerminalClause)
                {
                    result = new SyntaxParseResult<IN, OUT>();
                    result.AddErrors(innerResult.GetErrors());
                    //result.IsError = true;
                    var children = new List<ISyntaxNode<IN, OUT>> { innerResult.Root };
                    if (innerResult.IsError) children.Clear();
                    var node = new OptionSyntaxNode<IN, OUT>(nonTerminalClause.NonTerminalName, children,
                        null);
                    node.IsGroupOption = nonTerminalClause.IsGroup;
                    result.Root = node;
                    result.EndingPosition = state.Position;
                    state.Parent.SetResult(result);
                    ;
                }
                else if (state.OptionalClause is ChoiceClause<IN, OUT> choiceClause)
                {
                    if (choiceClause.IsTerminalChoice)
                    {
                        result.Root = new SyntaxLeaf<IN, OUT>(Token<IN>.Empty(), false);
                        state.Parent.SetResult(result);
                    }
                    else if (choiceClause.IsNonTerminalChoice)
                    {
                        result = new SyntaxParseResult<IN, OUT>();
                        result.AddErrors(innerResult.GetErrors());
                        //result.IsError = true;
                        var children = new List<ISyntaxNode<IN, OUT>> { innerResult.Root };
                        if (innerResult.IsError) children.Clear();
                        var node = new OptionSyntaxNode<IN, OUT>("", children,
                            null);
                        node.IsGroupOption = false;
                        result.Root = node;
                        result.EndingPosition = state.Position;
                        state.Parent.SetResult(result);
                    }
                }
            }
        }
    }

    private void ParseChoice(ChoiceStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        // match has been found return immediatly
        if (state.Result != null && state.Result.IsOk)
        {
            state.Parent.SetResult(state.Result);
            state.Index = 0;
            return;
        }

        // no more choice => failing
        if (state.Index >= state.Choice.Choices.Count)
        {
            SyntaxParseResult<IN, OUT> result = new SyntaxParseResult<IN, OUT>();
            result.IsError = true;
            result.EndingPosition = state.Result.EndingPosition;
            LeadingToken<IN>[] expected = [];
            if (state.Choice.IsTerminalChoice)
            {
                expected = state.Choice.Choices.Cast<TerminalClause<IN, OUT>>().Select(x => x.ExpectedToken)
                    .ToArray();
            }
            else if (state.Choice.IsNonTerminalChoice)
            {
                var nonTerminals = state.Choice.Choices.Cast<NonTerminalClause<IN, OUT>>();
                expected = nonTerminals.Select(x => Configuration.NonTerminals[x.NonTerminalName])
                    .SelectMany(y => y.Rules)
                    .SelectMany(z => z.PossibleLeadingTokens)
                    .ToArray();
            }

            result.AddError(
                new UnexpectedTokenSyntaxError<IN>(state.Tokens[state.Position], LexemeLabels, I18n, expected));
            state.Parent.SetResult(result);
            return;
        }


        state.Index++;
        stack.Push(state);
        if (state.Index - 1 < 0 || state.Index - 1 >= state.Choice.Choices.Count)
        {
            ;
        }

        var next = state.Choice.Choices[state.Index - 1];
        if (next is TerminalClause<IN, OUT> terminalClause)
        {
            terminalClause.Discarded = state.Choice.IsDiscarded;
        }

        PushClause(next, stack, state);
    }


    private void ParseZeroOrMore(ZeroOrMoreStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        // either first time we evaluate sub clause or previous evaluation is ok
        if (state.Result == null || state.IsOk)
        {
            state.Position = state.Children.Count > 0 ? state.Children.Last().EndingPosition : state.Position;
            // push self 
            state.Index++;
            stack.Push(state);

            // keep on trying 
            PushClause(state.RepeatedClause, stack, state);
            return;
        }
        else
        {
            // TODO : no more match return
            var result = new SyntaxParseResult<IN, OUT>();
            var manyNode = new ManySyntaxNode<IN, OUT>($"{state.RepeatedClause.ToString()}*");

            manyNode.IsManyGroups = state.RepeatedClause is NonTerminalClause<IN, OUT> nt && nt.IsGroup;
            if (!manyNode.IsManyGroups)
            {
                manyNode.IsManyTokens = state.RepeatedClause is TerminalClause<IN, OUT> ||
                                        state.RepeatedClause is ChoiceClause<IN, OUT> tc && tc.IsTerminalChoice;
                manyNode.IsManyValues = state.RepeatedClause is NonTerminalClause<IN, OUT> ||
                                        state.RepeatedClause is ChoiceClause<IN, OUT> ntc && ntc.IsNonTerminalChoice;
            }


            if (state.Children.Any())
            {
                foreach (var child in state.Children)
                {
                    if (child.Root != null)
                    {
                        result.EndingPosition = Math.Max(result.EndingPosition, child.EndingPosition);
                        manyNode.Add(child.Root);
                    }
                }
            }
            else
            {
                ;
                result.EndingPosition = state.Result.EndingPosition;
            }


            result.Root = manyNode;
            state.Parent.SetResult(result);
        }
    }

    private void ParseOneOrMore(OneOrMoreStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        // either first time we evaluate sub clause or previous evaluation is ok
        if (state.Result == null || state.IsOk)
        {
            state.Position = state.Children.Count > 0 ? state.Children.Last().EndingPosition : state.Position;
            state.Index++;
            // push self 
            stack.Push(state);

            // keep on trying 
            PushClause(state.RepeatedClause, stack, state);
            return;
        }
        else
        {
            // TODO : no more match return
            if (state.Children.Count == 0)
            {
                // error expecting at least one ...
                state.Parent.SetResult(state.Result);
                return;
            }

            var result = new SyntaxParseResult<IN, OUT>();
            var manyNode = new ManySyntaxNode<IN, OUT>($"{state.RepeatedClause.ToString()}*");
            manyNode.IsManyTokens = state.RepeatedClause is TerminalClause<IN, OUT>;
            manyNode.IsManyValues = state.RepeatedClause is NonTerminalClause<IN, OUT>;
            manyNode.IsManyGroups = state.RepeatedClause is NonTerminalClause<IN, OUT> nt && nt.IsGroup;

            if (state.RepeatedClause is ChoiceClause<IN, OUT> choice)
            {
                manyNode.IsManyGroups = false;
                manyNode.IsManyTokens = choice.IsTerminalChoice;
                manyNode.IsManyValues = choice.IsNonTerminalChoice;
            }

            foreach (var child in state.Children)
            {
                if (child.Root != null)
                {
                    result.EndingPosition = Math.Max(result.EndingPosition, child.EndingPosition);
                    manyNode.Add(child.Root);
                }
            }

            result.Root = manyNode;
            state.Parent.SetResult(result);
        }
    }

    private void ParseInfixExpressionRule(InfixExpressionStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        if (state.ExpressionState == ExpressionRuleState.NotStarted)
        {
            state.ExpressionState = ExpressionRuleState.Left;
            stack.Push(state);
            var nextClause = state.Rule.Clauses[0];
            PushClause(nextClause, stack, state);   
            return;
        }
        if (state.Result.IsOk)
        {
            if (state.ExpressionState == ExpressionRuleState.Done)
            {
                var children = new List<ISyntaxNode<IN, OUT>>();
                children.Add(state.Left.Root);
                children.Add(state.Operator.Root);
                children.Add(state.Right.Root);
                int currentPosition = state.Right.EndingPosition;
                var node = new SyntaxNode<IN, OUT>(state.Rule.NodeName ?? state.Rule.NonTerminalName, children);
                node.ExpressionAffix = state.Rule.ExpressionAffix;
                node = ExpressionRuleManager<IN, OUT>.ManageExpressionRules(state.Rule, node);
                node.IsByPassNode = state.Rule.IsByPassRule;
                var op = (state.Operator.Root as SyntaxLeaf<IN, OUT>).Token;
                var key = op.IsExplicit ? op.Value : op.TokenID.ToString();
                node.Operation = state.Rule.GetOperation(key);
                var finalResult = new SyntaxParseResult<IN, OUT>();
                finalResult.Root = node;
                finalResult.IsEnded = currentPosition >= state.Tokens.Length - 1
                                      || currentPosition == state.Tokens.Length - 2 &&
                                      state.Tokens[state.Tokens.Length - 1].IsEOS;
                finalResult.EndingPosition = currentPosition;
                state.Parent.SetResult(finalResult);
                return;
            }

            if (state.ExpressionState == ExpressionRuleState.Left)
            {
                state.Left = state.Result;
                state.Position = state.Left.EndingPosition;
                state.ExpressionState = ExpressionRuleState.Operator;
                stack.Push(state);
                var nextClause = state.Rule.Clauses[1];
                PushClause(nextClause, stack, state);
                return;
            }
            if (state.ExpressionState == ExpressionRuleState.Operator)
            {
                state.Operator = state.Result;
                state.ExpressionState = ExpressionRuleState.Right;
                state.Position = state.Operator.EndingPosition;
                stack.Push(state);
                var nextClause = state.Rule.Clauses[2];
                PushClause(nextClause, stack, state);
                return;
            }
            if (state.ExpressionState == ExpressionRuleState.Right)
            {
                state.Right = state.Result;
                state.ExpressionState = ExpressionRuleState.Done;
                state.Position =  state.Right.EndingPosition;
                stack.Push(state);
                return;
            }
        }
        else
        {
            if (state.Result.IsError)
            {
                if (state.ExpressionState == ExpressionRuleState.Operator)
                {
                    // return Left (ok)
                    state.Parent.SetResult(state.Left);
                    return;
                }
                
                if (state.ExpressionState == ExpressionRuleState.Right)
                {
                    // fail on operator parsing => return expression with left operand
                    state.Parent.SetResult(state.Left);
                    return;
                }
                
                // fail otherwise => return current result (left if left or right if done)
                state.Parent.SetResult(state.Result);
                return;
            }
        }
    }

    public override bool IsExtension(GrammarNode<IN, OUT> clause)
    {
        if (clause is Rule<IN, OUT> rule && rule.IsInfixExpressionRule)
        {
            // only infix . prefix is a regular rule and postfix are managed through 2 generic rules
            return true;
        }
        if (clause is Rule<IN, OUT> rule2 && rule2.IsExpressionRule)
        {
            ;
        } 

        return (clause is OptionClause<IN, OUT> || clause is ManyClause<IN, OUT> || clause is ChoiceClause<IN, OUT>);
    }

    public override void PushClauseExtension(GrammarNode<IN, OUT> clause, Stack<StackState<IN, OUT>> stack,
        StackState<IN, OUT> parent)
    {
        switch (clause)
        {
            case Rule<IN, OUT> rule when rule.IsInfixExpressionRule:
            {
                var state = new InfixExpressionStackState<IN, OUT>(rule, parent)
                {
                    Tokens = parent.Tokens,
                    Position = parent.Position
                };
                stack.Push(state);
                break;
            }
            case ZeroOrMoreClause<IN, OUT> zeroOrMore:
            {
                var state = new ZeroOrMoreStackState<IN, OUT>(zeroOrMore, parent)
                {
                    Tokens = parent.Tokens,
                    Position = parent.Position
                };
                stack.Push(state);
                break;
            }
            case OneOrMoreClause<IN, OUT> oneOrMore:
            {
                var state = new OneOrMoreStackState<IN, OUT>(oneOrMore, parent)
                {
                    Tokens = parent.Tokens,
                    Position = parent.Position
                };
                stack.Push(state);
                break;
            }
            case OptionClause<IN, OUT> option:
            {
                var state = new OptionStackState<IN, OUT>(option, parent)
                {
                    Tokens = parent.Tokens,
                    Position = parent.Position
                };
                stack.Push(state);
                break;
            }
            case ChoiceClause<IN, OUT> choice:
            {
                var state = new ChoiceStackState<IN, OUT>(choice, parent)
                {
                    Tokens = parent.Tokens,
                    Position = parent.Position
                };
                stack.Push(state);
                break;
            }
        }
    }
}