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

        var result = new SyntaxParseResult<IN, OUT>();
        var manyNode = new ManySyntaxNode<IN, OUT>($"{clause.Clause.ToString()}*");
        var currentPosition = position;
        var innerClause = clause.Clause;
        var stillOk = true;


        SyntaxParseResult<IN, OUT> lastInnerResult = null;

        // Optimization: Pre-allocate error list with reasonable capacity
        var innerErrors = new List<UnexpectedTokenSyntaxError<IN>>(8);

        bool hasByPasNodes = false;
        
        // Optimization: Check bounds once before loop
        var tokensLength = tokens.Length;
        
        while (stillOk && currentPosition < tokensLength)
        {
            SyntaxParseResult<IN, OUT> innerResult = null;
            switch (innerClause)
            {
                case TerminalClause<IN, OUT> term:
                    manyNode.IsManyTokens = true;
                    innerResult = ParseTerminal(tokens, term, currentPosition, parsingContext);
                    hasByPasNodes |= innerResult.HasByPassNodes;
                    break;
                case NonTerminalClause<IN, OUT> nonTerm:
                {
                    innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                    hasByPasNodes |= innerResult.HasByPassNodes;
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
                    hasByPasNodes |= innerResult.HasByPassNodes;
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
            }

            if (innerResult != null && !innerResult.IsError)
            {
                manyNode.Add(innerResult.Root);
                currentPosition = innerResult.EndingPosition;
                lastInnerResult = innerResult;
                hasByPasNodes |= innerResult.HasByPassNodes;
                var lastInnerErrors = lastInnerResult.GetErrors();
                if (lastInnerErrors != null && lastInnerErrors.Count > 0)
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
                stillOk = false;
            }
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