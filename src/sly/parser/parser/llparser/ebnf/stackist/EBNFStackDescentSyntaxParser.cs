using System;
using System.Collections.Generic;
using System.Linq;
using sly.parser.generator;
using sly.parser.llparser.bnf.stackist;
using sly.parser.parser.llparser.ebnf.stackist.state;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.parser.llparser.ebnf.stackist;

public class EBNFStackDescentSyntaxParser<IN,OUT> : StackDescentSyntaxParser<IN,OUT> where IN : struct, Enum
{
    public EBNFStackDescentSyntaxParser(string i18n, ParserConfiguration<IN, OUT> configuration)
    {
        I18n = i18n;
        Configuration = configuration;
    }
    
    public virtual void ParseExtension(StackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        switch (state)
        {
            case ZeroOrMoreStackState<IN, OUT> zeroOrmMore:
            {
                ParseZeroOrMore(zeroOrmMore, stack);
                break;
            }
        }
    }

    private void ParseZeroOrMore(ZeroOrMoreStackState<IN, OUT> state, Stack<StackState<IN, OUT>> stack)
    {
        // TODO 
        if (state.IsOk)
        {
            state.Position = state.Children.Last().EndingPosition;
            // push self 
            stack.Push(state);
            
            // keep on trying 
            PushClause(state.RepeatedClause, stack, state);
            return;
        }
        else
        {
            // TODO : create a node with children
            var result = new SyntaxParseResult<IN, OUT>();
            var manyNode = new ManySyntaxNode<IN, OUT>($"{state.RepeatedClause.ToString()}*");
            foreach (var child in state.Children)
            {
                manyNode.Add(child.Root);                
            }

            result.Root = manyNode;
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
        }
    }
}