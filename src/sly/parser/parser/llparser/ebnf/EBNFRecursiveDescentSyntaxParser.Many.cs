using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.ebnf;

public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT> where IN : struct, Enum
{
    #region parsing

    public SyntaxParseResult<IN, OUT> ParseZeroOrMore(Token<IN>[] tokens, ZeroOrMoreClause<IN, OUT> clause, int position,
        SyntaxParsingContext<IN, OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
        {
            return parseResult;
        }

        if (Configuration.CaptureAmbiguities)
        {
            var innerClauseAmbi = clause.Clause;
            var paths = new List<(List<ISyntaxNode<IN, OUT>> children, int position, bool hasByPassNodes)>
            {
                (new List<ISyntaxNode<IN, OUT>>(), position, false)
            };
            var innerErrorsAmbi = new List<UnexpectedTokenSyntaxError<IN>>();

            bool isManyTokens = false;
            bool isManyValues = false;
            bool isManyGroups = false;
            if (innerClauseAmbi is TerminalClause<IN, OUT>)
            {
                isManyTokens = true;
            }
            else if (innerClauseAmbi is NonTerminalClause<IN, OUT> nonTermClause)
            {
                isManyGroups = nonTermClause.IsGroup;
                isManyValues = !nonTermClause.IsGroup;
            }
            else if (innerClauseAmbi is ChoiceClause<IN, OUT> choiceClause)
            {
                isManyTokens = choiceClause.IsTerminalChoice;
                isManyValues = choiceClause.IsNonTerminalChoice;
            }

            while (true)
            {
                var newPaths = new List<(List<ISyntaxNode<IN, OUT>> children, int position, bool hasByPassNodes)>();
                foreach (var path in paths)
                {
                    SyntaxParseResult<IN, OUT> innerResult = null;
                    switch (innerClauseAmbi)
                    {
                        case TerminalClause<IN, OUT> term:
                            innerResult = ParseTerminal(tokens, term, path.position, parsingContext);
                            break;
                        case NonTerminalClause<IN, OUT> nonTerm:
                            innerResult = ParseNonTerminal(tokens, nonTerm, path.position, parsingContext);
                            break;
                        case ChoiceClause<IN, OUT> choice:
                            innerResult = ParseChoice(tokens, choice, path.position, parsingContext);
                            break;
                        default:
                            throw new InvalidOperationException("unable to apply repeater to " + innerClauseAmbi.GetType().Name);
                    }

                    if (innerResult != null && !innerResult.IsError && innerResult.EndingPosition > path.position)
                    {
                        var resultsToProcess = innerResult.AllResults ?? new List<SyntaxParseResult<IN, OUT>> { innerResult };
                        foreach (var res in resultsToProcess)
                        {
                            var alternatives = res.AlternativeRoots;
                            if (alternatives == null || alternatives.Count == 0)
                            {
                                alternatives = new List<ISyntaxNode<IN, OUT>> { res.Root };
                            }

                            foreach (var alt in alternatives)
                            {
                                var newChildren = new List<ISyntaxNode<IN, OUT>>(path.children) { alt };
                                var newHasByPassNodes = path.hasByPassNodes || res.HasByPassNodes;
                                newPaths.Add((newChildren, res.EndingPosition, newHasByPassNodes));
                            }

                            if (res.GetErrors() != null)
                            {
                                innerErrorsAmbi.AddRange(res.GetErrors());
                            }
                        }
                    }
                    else if (innerResult != null)
                    {
                        innerErrorsAmbi.AddRange(innerResult.GetErrors());
                    }
                }

                if (newPaths.Count == 0)
                {
                    break;
                }

                paths = newPaths;
            }

            var ambiguousResult = new SyntaxParseResult<IN, OUT>();
            var resultsByPosition = paths.GroupBy(p => p.position).ToList();
            var results = new List<SyntaxParseResult<IN, OUT>>();

            foreach (var group in resultsByPosition)
            {
                var pos = group.Key;
                var res = new SyntaxParseResult<IN, OUT>
                {
                    EndingPosition = pos,
                    IsError = false,
                    AlternativeRoots = new List<ISyntaxNode<IN, OUT>>(),
                    HasByPassNodes = group.Any(p => p.hasByPassNodes)
                };

                foreach (var path in group)
                {
                    var manyNodeAlt = new ManySyntaxNode<IN, OUT>($"{innerClauseAmbi}*")
                    {
                        IsManyTokens = isManyTokens,
                        IsManyValues = isManyValues,
                        IsManyGroups = isManyGroups
                    };
                    manyNodeAlt.Children.AddRange(path.children);
                    res.AlternativeRoots.Add(manyNodeAlt);
                }

                res.IsEnded = pos >= tokens.Length || (pos < tokens.Length && tokens[pos].IsEOS);
                results.Add(res);
            }

            if (results.Count > 0)
            {
                var best = results.OrderByDescending(r => r.EndingPosition).First();
                ambiguousResult.AlternativeRoots = best.AlternativeRoots;
                ambiguousResult.EndingPosition = best.EndingPosition;
                ambiguousResult.IsEnded = best.IsEnded;
                ambiguousResult.HasByPassNodes = best.HasByPassNodes;
                ambiguousResult.AllResults = results;
            }
            else
            {
                ambiguousResult.IsError = true;
                ambiguousResult.EndingPosition = position;
            }

            ambiguousResult.AddErrors(innerErrorsAmbi);
            parsingContext.Memoize(clause, position, ambiguousResult);
            return ambiguousResult;
        }

        var result = new SyntaxParseResult<IN, OUT>();
        var manyNode = new ManySyntaxNode<IN, OUT>($"{clause.Clause.ToString()}*");
        var currentPosition = position;
        var innerClause = clause.Clause;
        var stillOk = true;


        SyntaxParseResult<IN, OUT> lastInnerResult = null;

        var innerErrors = new List<UnexpectedTokenSyntaxError<IN>>();

        bool hasByPasNodes = false;
        while (stillOk)
        {
            SyntaxParseResult<IN, OUT> innerResult = null;
            switch (innerClause)
            {
                case TerminalClause<IN, OUT> term:
                    manyNode.IsManyTokens = true;
                    innerResult = ParseTerminal(tokens, term, currentPosition, parsingContext);
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                    break;
                case NonTerminalClause<IN, OUT> nonTerm:
                {
                    innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                    if (nonTerm.IsGroup)
                        manyNode.IsManyGroups = true;
                    else
                        manyNode.IsManyValues = true;
                    break;
                }
                case ChoiceClause<IN, OUT> choice:
                    manyNode.IsManyTokens = choice.IsTerminalChoice;
                    manyNode.IsManyValues = choice.IsNonTerminalChoice;
                    innerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
            }

            if (innerResult != null && !innerResult.IsError)
            {
                manyNode.Add(innerResult.Root);
                currentPosition = innerResult.EndingPosition;
                lastInnerResult = innerResult;
                hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                var lastInnerErrors = lastInnerResult.GetErrors();
                if (lastInnerErrors != null)
                {
                    innerErrors.AddRange(lastInnerErrors);
                }
            }
            else
            {
                if (innerResult != null)
                {
                    innerErrors.AddRange(innerResult.GetErrors());
                }
            }

            stillOk = innerResult != null && !innerResult.IsError && currentPosition < tokens.Length;
        }


        result.EndingPosition = currentPosition;
        result.IsError = false;
        result.AddErrors(innerErrors);
        result.Root = manyNode;
        result.IsEnded = lastInnerResult != null && lastInnerResult.IsEnded;
        result.HasByPassNodes = hasByPasNodes;
        parsingContext.Memoize(clause, position, result);
        return result;
    }

    public SyntaxParseResult<IN, OUT> ParseRepeat(Token<IN>[] tokens, RepeatClause<IN, OUT> clause, int position,
        SyntaxParsingContext<IN, OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
        {
            return parseResult;
        }
        var result = new SyntaxParseResult<IN, OUT>();
        var manyNode = new ManySyntaxNode<IN, OUT>($"{clause.Clause.ToString()}_{clause.DumpRange().Replace("{","").Replace("}","").Replace("-","_")}");
        
        var currentPosition = position;
        var innerClause = clause.Clause;
        SyntaxParseResult<IN, OUT> innerResult = null;
        bool hasByPasNodes = false;

        List<UnexpectedTokenSyntaxError<IN>> innerErrors = new List<UnexpectedTokenSyntaxError<IN>>();
        
        for (int i = 0; i < clause.MaxRepetitionCount; i++)
        {
            innerResult = ParseInnerRepeat(tokens, parsingContext, innerClause, manyNode, currentPosition, out hasByPasNodes);
            
            
            if (innerResult.IsError && clause.MinRepetitionCount != 0 && i != 0)
            {
                result.IsError = true;
                result.AddErrors(innerResult.GetErrors());
                
                break;
            }

            var errors = innerResult.GetErrors();
            if (errors != null && errors.Any())
            {
                innerErrors.AddRange(innerResult.GetErrors());
                break;
            }

            manyNode.Add(innerResult.Root);
            currentPosition = innerResult.EndingPosition;
        }

        bool isRangeError = false;
        if (manyNode.Children.Count < clause.MinRepetitionCount)
        {
            result.IsError = true;
            isRangeError = true;
            result.AddErrors(innerErrors);
        }
        
        result.EndingPosition = currentPosition;
        result.IsError = isRangeError;
        result.AddErrors(innerErrors);
        result.Root = manyNode;
        result.IsEnded = innerResult != null && innerResult.IsEnded;
        result.HasByPassNodes = hasByPasNodes;
        parsingContext.Memoize(clause, position, result);
        
        
        return result;
    }

    private SyntaxParseResult<IN, OUT> ParseInnerRepeat(Token<IN>[] tokens, SyntaxParsingContext<IN, OUT> parsingContext, IClause<IN, OUT> innerClause,
        ManySyntaxNode<IN, OUT> manyNode, int currentPosition, out bool hasByPasNodes)
    {
        SyntaxParseResult<IN, OUT> innerResult;
        switch (innerClause)
        {
            case TerminalClause<IN, OUT> terminalClause:
                manyNode.IsManyTokens = true;
                innerResult = ParseTerminal(tokens, terminalClause, currentPosition, parsingContext);
                hasByPasNodes = innerResult.HasByPassNodes;
                break;
            case NonTerminalClause<IN, OUT> nonTerm:
            {
                innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                hasByPasNodes = innerResult.HasByPassNodes;
                if (nonTerm.IsGroup)
                    manyNode.IsManyGroups = true;
                else
                    manyNode.IsManyValues = true;
                break;
            }
            case ChoiceClause<IN, OUT> choice:
                manyNode.IsManyTokens = choice.IsTerminalChoice;
                manyNode.IsManyValues = choice.IsNonTerminalChoice;
                innerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                hasByPasNodes = innerResult.HasByPassNodes;
                break;
            default:
                throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
        }

        return innerResult;
    }

    public SyntaxParseResult<IN, OUT> ParseOneOrMore(Token<IN>[] tokens, OneOrMoreClause<IN, OUT> clause, int position,
        SyntaxParsingContext<IN, OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
        {
            return parseResult;
        }

        if (Configuration.CaptureAmbiguities)
        {
            var innerClauseAmbi = clause.Clause;
            var innerErrorsAmbi = new List<UnexpectedTokenSyntaxError<IN>>();
            var paths = new List<(List<ISyntaxNode<IN, OUT>> children, int position, bool hasByPassNodes)>();

            bool isManyTokens = false;
            bool isManyValues = false;
            bool isManyGroups = false;
            if (innerClauseAmbi is TerminalClause<IN, OUT>)
            {
                isManyTokens = true;
            }
            else if (innerClauseAmbi is NonTerminalClause<IN, OUT> nonTermClause)
            {
                isManyGroups = nonTermClause.IsGroup;
                isManyValues = !nonTermClause.IsGroup;
            }
            else if (innerClauseAmbi is ChoiceClause<IN, OUT> choiceClause)
            {
                isManyTokens = choiceClause.IsTerminalChoice;
                isManyValues = choiceClause.IsNonTerminalChoice;
            }

            SyntaxParseResult<IN, OUT> firstResult = null;
            switch (innerClauseAmbi)
            {
                case TerminalClause<IN, OUT> terminalClause:
                    firstResult = ParseTerminal(tokens, terminalClause, position, parsingContext);
                    break;
                case NonTerminalClause<IN, OUT> nonTerm:
                    firstResult = ParseNonTerminal(tokens, nonTerm, position, parsingContext);
                    break;
                case ChoiceClause<IN, OUT> choice:
                    firstResult = ParseChoice(tokens, choice, position, parsingContext);
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater to " + innerClauseAmbi.GetType().Name);
            }

            if (firstResult == null || firstResult.IsError)
            {
                var errorResult = new SyntaxParseResult<IN, OUT>
                {
                    IsError = true,
                    EndingPosition = position
                };
                if (firstResult != null)
                {
                    errorResult.AddErrors(firstResult.GetErrors());
                }
                parsingContext.Memoize(clause, position, errorResult);
                return errorResult;
            }

            var firstResults = firstResult.AllResults ?? new List<SyntaxParseResult<IN, OUT>> { firstResult };
            foreach (var res in firstResults)
            {
                if (res.GetErrors() != null)
                {
                    innerErrorsAmbi.AddRange(res.GetErrors());
                }

                var alternatives = res.AlternativeRoots;
                if (alternatives == null || alternatives.Count == 0)
                {
                    alternatives = new List<ISyntaxNode<IN, OUT>> { res.Root };
                }

                foreach (var alt in alternatives)
                {
                    var baseChildren = new List<ISyntaxNode<IN, OUT>> { alt };
                    var more = new ZeroOrMoreClause<IN, OUT>(innerClauseAmbi);
                    var moreResult = ParseZeroOrMore(tokens, more, res.EndingPosition, parsingContext);
                    var moreResults = moreResult?.AllResults ?? new List<SyntaxParseResult<IN, OUT>> { moreResult };

                    foreach (var moreRes in moreResults)
                    {
                        if (moreRes == null || moreRes.IsError)
                        {
                            continue;
                        }

                        var manyAlternatives = moreRes.AlternativeRoots;
                        if (manyAlternatives == null || manyAlternatives.Count == 0)
                        {
                            manyAlternatives = new List<ISyntaxNode<IN, OUT>> { moreRes.Root };
                        }

                        foreach (var manyAlt in manyAlternatives)
                        {
                            var combinedChildren = new List<ISyntaxNode<IN, OUT>>(baseChildren);
                            if (manyAlt is ManySyntaxNode<IN, OUT> manyNodeAlt)
                            {
                                combinedChildren.AddRange(manyNodeAlt.Children);
                            }
                            else
                            {
                                combinedChildren.Add(manyAlt);
                            }

                            var combinedByPass = res.HasByPassNodes || moreRes.HasByPassNodes;
                            paths.Add((combinedChildren, moreRes.EndingPosition, combinedByPass));
                        }

                        if (moreRes.GetErrors() != null)
                        {
                            innerErrorsAmbi.AddRange(moreRes.GetErrors());
                        }
                    }
                }
            }

            var ambiguousResult = new SyntaxParseResult<IN, OUT>();
            var resultsByPosition = paths.GroupBy(p => p.position).ToList();
            var results = new List<SyntaxParseResult<IN, OUT>>();

            foreach (var group in resultsByPosition)
            {
                var pos = group.Key;
                var res = new SyntaxParseResult<IN, OUT>
                {
                    EndingPosition = pos,
                    IsError = false,
                    AlternativeRoots = new List<ISyntaxNode<IN, OUT>>(),
                    HasByPassNodes = group.Any(p => p.hasByPassNodes)
                };

                foreach (var path in group)
                {
                    var manyNodeAlt = new ManySyntaxNode<IN, OUT>($"{innerClauseAmbi}+")
                    {
                        IsManyTokens = isManyTokens,
                        IsManyValues = isManyValues,
                        IsManyGroups = isManyGroups
                    };
                    manyNodeAlt.Children.AddRange(path.children);
                    res.AlternativeRoots.Add(manyNodeAlt);
                }

                res.IsEnded = pos >= tokens.Length || (pos < tokens.Length && tokens[pos].IsEOS);
                results.Add(res);
            }

            if (results.Count > 0)
            {
                var best = results.OrderByDescending(r => r.EndingPosition).First();
                ambiguousResult.AlternativeRoots = best.AlternativeRoots;
                ambiguousResult.EndingPosition = best.EndingPosition;
                ambiguousResult.IsEnded = best.IsEnded;
                ambiguousResult.HasByPassNodes = best.HasByPassNodes;
                ambiguousResult.AllResults = results;
            }
            else
            {
                ambiguousResult.IsError = true;
                ambiguousResult.EndingPosition = position;
            }

            ambiguousResult.AddErrors(innerErrorsAmbi);
            parsingContext.Memoize(clause, position, ambiguousResult);
            return ambiguousResult;
        }

        var result = new SyntaxParseResult<IN, OUT>();
        var manyNode = new ManySyntaxNode<IN, OUT>($"{clause.Clause.ToString()}+");
        var currentPosition = position;
        var innerClause = clause.Clause;
        bool isError;

        SyntaxParseResult<IN, OUT> lastInnerResult = null;

        bool hasByPasNodes = false;
        SyntaxParseResult<IN, OUT> firstInnerResult = null;
        var innerErrors = new List<UnexpectedTokenSyntaxError<IN>>();

        switch (innerClause)
        {
            case TerminalClause<IN, OUT> terminalClause:
                manyNode.IsManyTokens = true;
                firstInnerResult = ParseTerminal(tokens, terminalClause, currentPosition, parsingContext);
                hasByPasNodes = firstInnerResult.HasByPassNodes;
                break;
            case NonTerminalClause<IN, OUT> nonTerm:
            {
                firstInnerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                hasByPasNodes = firstInnerResult.HasByPassNodes;
                if (nonTerm.IsGroup)
                    manyNode.IsManyGroups = true;
                else
                    manyNode.IsManyValues = true;
                break;
            }
            case ChoiceClause<IN, OUT> choice:
                manyNode.IsManyTokens = choice.IsTerminalChoice;
                manyNode.IsManyValues = choice.IsNonTerminalChoice;
                firstInnerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                hasByPasNodes = firstInnerResult.HasByPassNodes;
                break;
            default:
                throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
        }

        if (!firstInnerResult.IsError)
        {
            manyNode.Add(firstInnerResult.Root);
            lastInnerResult = firstInnerResult;
            currentPosition = firstInnerResult.EndingPosition;
            var more = new ZeroOrMoreClause<IN, OUT>(innerClause);
            var nextResult = ParseZeroOrMore(tokens, more, currentPosition, parsingContext);
            if (nextResult != null && !nextResult.IsError)
            {
                currentPosition = nextResult.EndingPosition;
                var moreChildren = (ManySyntaxNode<IN, OUT>)nextResult.Root;
                manyNode.Children.AddRange(moreChildren.Children);
            }

            if (nextResult != null)
            {
                innerErrors.AddRange(nextResult.GetErrors());
            }

            isError = false;
        }
        else
        {
            innerErrors.AddRange(firstInnerResult.GetErrors());
            isError = true;
        }

        result.EndingPosition = currentPosition;
        result.IsError = isError;
        result.AddErrors(innerErrors);
        result.Root = manyNode;
        result.IsEnded = lastInnerResult != null && lastInnerResult.IsEnded;
        result.HasByPassNodes = hasByPasNodes;
        parsingContext.Memoize(clause, position, result);
        return result;
    }

    #endregion
}