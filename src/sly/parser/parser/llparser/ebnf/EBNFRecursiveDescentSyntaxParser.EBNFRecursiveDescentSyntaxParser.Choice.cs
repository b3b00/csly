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
        List<SyntaxParseResult<IN, OUT>> alternateResults = new List<SyntaxParseResult<IN, OUT>>();

        // Collect all results for all alternatives, even if CaptureAmbiguities is false (to determine the longest match in case of failure)
        foreach (var alternate in clause.Choices)
        {
            switch (alternate)
            {
                case TerminalClause<IN, OUT> terminalAlternate:
                    var rterm = ParseTerminal(tokens, terminalAlternate, currentPosition, parsingContext);
                    alternateResults.Add(rterm);
                    break;
                case NonTerminalClause<IN, OUT> nonTerminalAlternate:
                    var rnonterm = ParseNonTerminal(tokens, nonTerminalAlternate, currentPosition, parsingContext);
                    alternateResults.Add(rnonterm);
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater inside  " + clause.GetType().Name);
            }

            if (!Configuration.CaptureAmbiguities)
            {
                var lastResult = alternateResults.Last();
                if (lastResult.IsOk)
                {
                    // return immediately if we don't care about ambiguities, otherwise keep collecting results for all alternatives
                    if (clause.IsTerminalChoice && clause.IsDiscarded && lastResult.Root is SyntaxLeaf<IN, OUT> leaf)
                    {
                        var discardedToken = new SyntaxLeaf<IN, OUT>(leaf.Token, true);
                        lastResult.Root = discardedToken;
                    }
                    parsingContext.Memoize(clause, position, lastResult);
                    return lastResult;
                }
            }
        }

        // manage ambiguities.
        SyntaxParseResult<IN, OUT> result;
        
        if (Configuration.CaptureAmbiguities)
        {
            var okResults = alternateResults.Where(r => r.IsOk).ToList();
            
            if (okResults.Any())
            {
                // Toutes les alternatives réussies à la même position finale
                var maxLength = okResults.Max(r => r.EndingPosition);
                var maxResults = okResults.Where(r => r.EndingPosition == maxLength).ToList();
                
                if (maxResults.Count > 1 || maxResults.Any(r => r.HasAmbiguity))
                {
                    // AMBIGUÏTÉ dans le choix EBNF
                    result = new SyntaxParseResult<IN, OUT>
                    {
                        AlternativeRoots = maxResults.SelectMany(r => r.AlternativeRoots).ToList(),
                        EndingPosition = maxLength,
                        IsError = false,
                        AllResults = okResults
                    };
                    
                    // save ambiguity
                    result.Ambiguities = new List<sly.parser.syntax.tree.AmbiguityInfo<IN, OUT>>();
                    
                    if (maxResults.Count > 1) {
                        result.Ambiguities.Add(new sly.parser.syntax.tree.AmbiguityInfo<IN, OUT>
                        {
                            NonTerminalName = $"Choice[{string.Join("|", clause.Choices.Select((c, i) => $"alt{i}"))}]",
                            Position = position,
                            AlternativeCount = maxResults.Count
                        });
                    }
                    
                    foreach(var maxResult in maxResults) {
                        if (maxResult.Ambiguities != null) {
                            result.Ambiguities.AddRange(maxResult.Ambiguities);
                        }
                    }
                }
                else
                {
                    result = maxResults[0];
                    result.AllResults = okResults;
                }
                
                if (clause.IsTerminalChoice && clause.IsDiscarded && result.Root is SyntaxLeaf<IN, OUT> leaf)
                {
                    var discardedToken = new SyntaxLeaf<IN, OUT>(leaf.Token, true);
                    result.Root = discardedToken;
                }
            }
            else
            {
                // all alternatives failed, return the one with the longest match (to provide the most accurate error message)
                result = HandleAllChoicesFailed(clause, tokens, currentPosition, alternateResults);
            }
        }
        else
        {
            result = HandleAllChoicesFailed(clause, tokens, currentPosition, alternateResults);
        }

        parsingContext.Memoize(clause, position, result);
        return result;
    }
    
    private SyntaxParseResult<IN, OUT> HandleAllChoicesFailed(
        ChoiceClause<IN, OUT> clause, 
        Token<IN>[] tokens, 
        int currentPosition, 
        List<SyntaxParseResult<IN, OUT>> alternateResults)
    {
        var result = new SyntaxParseResult<IN, OUT>
        {
            IsError = true,
            IsEnded = false,
            EndingPosition = currentPosition
        };
        
        if (clause.IsTerminalChoice)
        {
            var terminalAlternates = clause.Choices.Cast<TerminalClause<IN, OUT>>();
            var expected = terminalAlternates.Select(x => x.ExpectedToken).ToList();
            result.AddError(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], LexemeLabels, I18n,
                expected.ToArray()));
        }
        else
        {
            var greaterPosition = alternateResults.Select(x => x.EndingPosition).Max();
            var errors = alternateResults.Where(x => x.EndingPosition == greaterPosition)
                .SelectMany(x => x.GetErrors()).ToList();
            result.AddErrors(errors);
        }
        
        return result;
    }

    #endregion
}