using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.strategies;

public class OneOrMoreParseStrategy<IN, OUT> : AbstractClauseParseStrategy<IN, OUT> where IN : struct
{
    public override SyntaxParseResult<IN> Parse(Rule<IN> rule, IClause<IN> clause, IList<Token<IN>> tokens,
        int position, SyntaxParsingContext<IN> parsingContext)
    {
        var oneOrMoreClause = clause as OneOrMoreClause<IN>;
       if (parsingContext.TryGetParseResult(oneOrMoreClause, position, out var parseResult))
            {
                return parseResult;
            }
            
            var result = new SyntaxParseResult<IN>();
            var manyNode = new ManySyntaxNode<IN>($"{oneOrMoreClause.Clause.ToString()}+");
            var currentPosition = position;
            var innerClause = oneOrMoreClause.Clause;
            bool isError;

            SyntaxParseResult<IN> lastInnerResult = null;

            bool hasByPasNodes = false;
            SyntaxParseResult<IN> firstInnerResult = null;
            var innerErrors = new List<UnexpectedTokenSyntaxError<IN>>();

            firstInnerResult = Strategist.Parse(rule, innerClause, tokens, position, parsingContext);
            hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
            
            
            // switch (innerClause)
            // {
            //     case TerminalClause<IN> terminalClause:
            //         manyNode.IsManyTokens = true;
            //         firstInnerResult = ParseTerminal(tokens, terminalClause, currentPosition, parsingContext);
            //         hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
            //         break;
            //     case NonTerminalClause<IN> nonTerm:
            //     {
            //         firstInnerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
            //         hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
            //         if (nonTerm.IsGroup)
            //             manyNode.IsManyGroups = true;
            //         else
            //             manyNode.IsManyValues = true;
            //         break;
            //     }
            //     case ChoiceClause<IN> choice:
            //         manyNode.IsManyTokens = choice.IsTerminalChoice;
            //         manyNode.IsManyValues = choice.IsNonTerminalChoice;
            //         firstInnerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
            //         hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
            //         break;
            //     default:
            //         throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
            // }

            if (firstInnerResult != null && !firstInnerResult.IsError)
            {
                manyNode.Add(firstInnerResult.Root);
                lastInnerResult = firstInnerResult;
                currentPosition = firstInnerResult.EndingPosition;
                var more = new ZeroOrMoreClause<IN>(innerClause);
                ZeroOrMoreParseStrategy<IN, OUT> zeroOrMoreParseStrategy = new ZeroOrMoreParseStrategy<IN, OUT>()
                {
                    Configuration = Configuration,
                    LexemeLabels = LexemeLabels,
                    I18n = I18n,
                    Strategist = Strategist
                };
                var nextResult = zeroOrMoreParseStrategy.Parse(rule, clause, tokens, position, parsingContext);
                    
                //var nextResult = ParseZeroOrMore(tokens, more, currentPosition, parsingContext);
                if (nextResult != null && !nextResult.IsError)
                {
                    currentPosition = nextResult.EndingPosition;
                    var moreChildren = (ManySyntaxNode<IN>) nextResult.Root;
                    manyNode.Children.AddRange(moreChildren.Children);
                }
                if (nextResult != null)
                {
                    innerErrors.AddRange(nextResult.Errors);
                }

                isError = false;
            }
            else
            {
                if (firstInnerResult != null)
                {
                    innerErrors.AddRange(firstInnerResult.Errors);
                }
                isError = true;
            }

            result.EndingPosition = currentPosition;
            result.IsError = isError;
            result.Errors = innerErrors;
            result.Root = manyNode;
            result.IsEnded = lastInnerResult != null && lastInnerResult.IsEnded;
            result.HasByPassNodes = hasByPasNodes;
            parsingContext.Memoize(oneOrMoreClause,position,result);
            return result;
    }
}
