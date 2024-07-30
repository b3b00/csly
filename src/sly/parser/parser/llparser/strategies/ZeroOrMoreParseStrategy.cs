using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.strategies;

public class ZeroOrMoreParseStrategy<IN, OUT> : AbstractClauseParseStrategy<IN, OUT> where IN : struct
{
    public override SyntaxParseResult<IN> Parse(Rule<IN> rule, IClause<IN> clause, IList<Token<IN>> tokens,
        int position, SyntaxParsingContext<IN> parsingContext)
    {
        var zeroOrMoreClause = clause as ZeroOrMoreClause<IN>;
        if (parsingContext.TryGetParseResult(zeroOrMoreClause, position, out var parseResult))
        {
            return parseResult;
        }

        var result = new SyntaxParseResult<IN>();
        var manyNode = new ManySyntaxNode<IN>($"{zeroOrMoreClause.Clause.ToString()}*");
        var currentPosition = position;
        var innerClause = zeroOrMoreClause.Clause;
        var stillOk = true;


        SyntaxParseResult<IN> lastInnerResult = null;

        var innerErrors = new List<UnexpectedTokenSyntaxError<IN>>();

        bool hasByPasNodes = false;
        while (stillOk)
        {
            SyntaxParseResult<IN> innerResult = null;
            innerResult = Strategist.Parse(innerClause, tokens, currentPosition, parsingContext);
            hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
            // switch (innerClause)
            // {
            //     case TerminalClause<IN> term:
            //         manyNode.IsManyTokens = true;
            //         innerResult = ParseTerminal(tokens, term, currentPosition, parsingContext);
            //         hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
            //         break;
            //     case NonTerminalClause<IN> nonTerm:
            //     {
            //         innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
            //         hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
            //         if (nonTerm.IsGroup)
            //             manyNode.IsManyGroups = true;
            //         else
            //             manyNode.IsManyValues = true;
            //         break;
            //     }
            //     case GroupClause<IN> _:
            //         manyNode.IsManyGroups = true;
            //         innerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<IN>, currentPosition,
            //             parsingContext);
            //         hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
            //         break;
            //     case ChoiceClause<IN> choice:
            //         manyNode.IsManyTokens = choice.IsTerminalChoice;
            //         manyNode.IsManyValues = choice.IsNonTerminalChoice;
            //         innerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
            //         hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
            //         break;
            //     default:
            //         throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
            // }

            if (innerResult != null && !innerResult.IsError)
            {
                manyNode.Add(innerResult.Root);
                currentPosition = innerResult.EndingPosition;
                lastInnerResult = innerResult;
                hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                innerErrors.AddRange(lastInnerResult.Errors);
            }
            else
            {
                if (innerResult != null)
                {
                    innerErrors.AddRange(innerResult.Errors);
                }
            }

            stillOk = innerResult != null && !innerResult.IsError && currentPosition < tokens.Count;
        }


        result.EndingPosition = currentPosition;
        result.IsError = false;
        result.Errors = innerErrors;
        result.Root = manyNode;
        result.IsEnded = lastInnerResult != null && lastInnerResult.IsEnded;
        result.HasByPassNodes = hasByPasNodes;
        parsingContext.Memoize(zeroOrMoreClause, position, result);
        return result;
    }
}
