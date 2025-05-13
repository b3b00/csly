using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.llparser.bnf.stackist;
using sly.parser.llparser.ebnf;
using sly.parser.parser.llparser.ebnf.stackist.state;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.parser.llparser.ebnf.stackist;

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
        switch (state)
        {
            case ZeroOrMoreStackState<IN, OUT> zeroOrmMore:
            {
                ParseZeroOrMore(zeroOrmMore, stack);
                break;
            }
            case OneOrMoreStackState<IN, OUT> oneOrMore:
            {
                ParseOneOrMore(oneOrMore, stack);
                break;
            }
            case OptionStackState<IN, OUT> option:
            {
                ParseOption(option, stack);
                break;
            }
            case ChoiceStackState<IN, OUT> choice:
            {
                ParseChoice(choice, stack);
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
                state.Parent.SetResult(state.Result);

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
            result.EndingPosition = state.Position;
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

        PushClause(state.Choice.Choices[state.Index - 1], stack, state);
    }


    private void ParseZeroOrMore(ZeroOrMoreStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        // either first time we evaluate sub clause or previous evaluation is ok
        if (state.Result == null || state.IsOk)
        {
            state.Position = state.Children.Count > 0 ? state.Children.Last().EndingPosition : state.Position;
            // push self 
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


            // TODO many groups
            foreach (var child in state.Children)
            {
                manyNode.Add(child.Root);                
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
            // TODO many groups
            foreach (var child in state.Children)
            {
                manyNode.Add(child.Root);                
            }

            result.Root = manyNode;
            state.Parent.SetResult(result);
        }
    }

    public override void PushClauseExtension(IClause<IN, OUT> clause, Stack<StackState<IN, OUT>> stack,
        StackState<IN, OUT> parent)
    {
        switch (clause)
        {
            case ZeroOrMoreClause<IN, OUT> zeroOrMore:
            {
                var state = new ZeroOrMoreStackState<IN,OUT>(zeroOrMore, parent)
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