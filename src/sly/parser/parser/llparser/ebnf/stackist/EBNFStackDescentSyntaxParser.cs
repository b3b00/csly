using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser.generator;
using sly.parser.llparser.bnf.stackist;
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
        }
    }

    private void ParseOption(OptionStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        if (state.Result == null)
        {
            PushClause(state.OptionalClause, stack, state);
        }
        else
        {
            var innerResult = state.Result;
            var result = new SyntaxParseResult<IN, OUT>();
            if (state.Result.IsOk)
            {
               // TODO : ok => Option(state.Result.Root)
            }
            else if (state.Result.IsError)
            {
                // TODO : empty node
            }
        }
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
        }
    }
}