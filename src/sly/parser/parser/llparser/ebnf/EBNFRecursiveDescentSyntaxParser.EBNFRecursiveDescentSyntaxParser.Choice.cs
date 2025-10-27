using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.ebnf;

public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT> where IN : struct, Enum
{
    #region parsing

    public SyntaxParseResult<IN, OUT> ParseChoice(Token<IN>[] tokens, ChoiceClause<IN, OUT> clause,
        int position, SyntaxParsingContext<IN, OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
        {
            return parseResult;
        }

        var currentPosition = position;

        SyntaxParseResult<IN, OUT> result = new SyntaxParseResult<IN, OUT>
        {
            IsError = true,
            IsEnded = false,
            EndingPosition = currentPosition
        };

        List<SyntaxParseResult<IN, OUT>> alternateResults = new List<SyntaxParseResult<IN, OUT>>(clause.Choices.Count);
        
        // Optimization: Early exit on first successful match
        foreach (var alternate in clause.Choices)
        {
            SyntaxParseResult<IN, OUT> currentResult;
            
            switch (alternate)
            {
                case TerminalClause<IN, OUT> terminalAlternate:
                    currentResult = ParseTerminal(tokens, terminalAlternate, currentPosition, parsingContext);
                    break;
                case NonTerminalClause<IN, OUT> nonTerminalAlternate:
                    currentResult = ParseNonTerminal(tokens, nonTerminalAlternate, currentPosition, parsingContext);
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater inside  " + clause.GetType().Name);
            }

            // Optimization: Exit immediately on success
            if (currentResult.IsOk)
            {
                if (clause.IsTerminalChoice && clause.IsDiscarded && currentResult.Root is SyntaxLeaf<IN, OUT> leaf)
                {
                    var discardedToken = new SyntaxLeaf<IN, OUT>(leaf.Token, true);
                    currentResult.Root = discardedToken;
                }

                parsingContext.Memoize(clause, position, currentResult);
                return currentResult;
            }
            
            alternateResults.Add(currentResult);
        }

        // here all alternateResult are KO - optimization: set result to last attempt
        result = alternateResults[alternateResults.Count - 1];
        
        if (clause.IsTerminalChoice)
        {
            var terminalAlternates = clause.Choices.Cast<TerminalClause<IN, OUT>>();
            var expected = terminalAlternates.Select(x => x.ExpectedToken).ToList();
            result.AddError(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], LexemeLabels, I18n,
                expected.ToArray()));
        }
        else
        {
            // Optimization: Use LINQ more efficiently
            var greaterPosition = alternateResults[0].EndingPosition;
            for (int i = 1; i < alternateResults.Count; i++)
            {
                if (alternateResults[i].EndingPosition > greaterPosition)
                {
                    greaterPosition = alternateResults[i].EndingPosition;
                }
            }
            
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            for (int i = 0; i < alternateResults.Count; i++)
            {
                if (alternateResults[i].EndingPosition == greaterPosition)
                {
                    errors.AddRange(alternateResults[i].GetErrors());
                }
            }
            
            result.AddErrors(errors);
            result.IsError = true;
        }

        parsingContext.Memoize(clause, position, result);
        return result;
    }

    #endregion
}