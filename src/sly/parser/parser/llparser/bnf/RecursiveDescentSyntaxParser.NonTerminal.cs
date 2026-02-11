using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf;

public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct, Enum
{
    #region parsing

    public SyntaxParseResult<IN, OUT> ParseNonTerminal(Token<IN>[] tokens, NonTerminalClause<IN, OUT> nonTermClause,
        int currentPosition, SyntaxParsingContext<IN, OUT> parsingContext)
    {
        var result = ParseNonTerminal(tokens, nonTermClause.NonTerminalName, currentPosition, parsingContext);
        return result;
    }

    public SyntaxParseResult<IN, OUT> ParseNonTerminal(Token<IN>[] tokens, string nonTerminalName,
        int currentPosition, SyntaxParsingContext<IN, OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(new NonTerminalClause<IN, OUT>(nonTerminalName), currentPosition,
                out var memoizedResult))
        {
            return memoizedResult;
        }

        var startPosition = currentPosition;
        var nt = Configuration.NonTerminals[nonTerminalName];
        var i = 0;
        var rules = nt.Rules;

        var rulesResults = new List<SyntaxParseResult<IN, OUT>>();
        while (i < rules.Count)
        {
            var innerrule = rules[i];
            if (startPosition < tokens.Length 
                && (!tokens[startPosition].IsEOS || (tokens[startPosition].IsEOS && innerrule.MayBeEmpty)) 
                && innerrule.Match(tokens, startPosition, Configuration))
            {
                var innerRuleRes = Parse(tokens, innerrule, startPosition, nonTerminalName, parsingContext);
                rulesResults.Add(innerRuleRes);
            }

            i++;
        }

        if (rulesResults.Count == 0)
        {
            var allAcceptableTokens = new List<LeadingToken<IN>>();
            nt.Rules.ForEach(r =>
            {
                if (r != null && r.PossibleLeadingTokens != null)
                    allAcceptableTokens.AddRange(r.PossibleLeadingTokens);
            });

            var noMatching = NoMatchingRuleError(tokens, currentPosition, allAcceptableTokens);
            parsingContext.Memoize(new NonTerminalClause<IN, OUT>(nonTerminalName), currentPosition, noMatching);
            return noMatching;
        }

        
        var result = new SyntaxParseResult<IN, OUT>();
        
        if (Configuration.CaptureAmbiguities)
        {
            // filter only ok results
            var okResults = rulesResults.Where(r => r.IsOk).ToList();
            
            if (okResults.Any())
            {
                result.AllResults = okResults;
                
                var maxLength = okResults.Max(r => r.EndingPosition);
                var maxResults = okResults.Where(r => r.EndingPosition == maxLength).ToList();
                
                if (maxResults.Count > 1 || maxResults.Any(r => r.HasAmbiguity))
                {
                    // ambiguity detected
                    result.AlternativeRoots = maxResults.SelectMany(r => r.AlternativeRoots).ToList();
                    result.EndingPosition = maxLength;
                    result.IsError = false;
                    result.IsEnded = maxResults[0].IsEnded;
                    result.HasByPassNodes = maxResults.Any(r => r.HasByPassNodes);
                    

                    // save ambiguity
                    if (result.Ambiguities == null)
                        result.Ambiguities = new List<sly.parser.syntax.tree.AmbiguityInfo<IN, OUT>>();
                        
                    if (maxResults.Count > 1) {
                        result.Ambiguities.Add(new sly.parser.syntax.tree.AmbiguityInfo<IN, OUT>
                        {
                            NonTerminalName = nonTerminalName,
                            Position = currentPosition,
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
                    // no ambiguity, take the single best result
                    result.Root = maxResults[0].Root;
                    result.EndingPosition = maxResults[0].EndingPosition;
                    result.IsError = false;
                    result.IsEnded = maxResults[0].IsEnded;
                    result.HasByPassNodes = maxResults[0].HasByPassNodes;
                }
            }
            else
            {
                // all results are errors, take the one that got the furthest
                var koEndingPosition = rulesResults.Max(r => r.EndingPosition);
                var maxKos = rulesResults.Where(r => r.EndingPosition == koEndingPosition).ToList();
                
                var maxKo = maxKos[0];
                result.Root = maxKo.Root;
                result.EndingPosition = maxKo.EndingPosition;
                result.IsError = true;
                result.IsEnded = maxKo.IsEnded;
                result.HasByPassNodes = maxKo.HasByPassNodes;
                
                foreach(var ko in maxKos) {
                    result.AddErrors(ko.GetErrors().ToArray());
                }
                result.AllResults.Add(result);
            }
        }
        else
        {
            // take the result that got the furthest, preferring ok results over errors
            SyntaxParseResult<IN, OUT> max = null;
            int okEndingPosition = -1;
            int koEndingPosition = -1;
            bool hasOk = false;
            SyntaxParseResult<IN, OUT> maxOk = null;
            SyntaxParseResult<IN, OUT> maxKo = null;
            foreach (var rulesResult in rulesResults)
            {
                if (rulesResult.IsOk)
                {
                    hasOk = true;
                    if (rulesResult.EndingPosition > okEndingPosition)
                    {
                        okEndingPosition = rulesResult.EndingPosition;
                        maxOk = rulesResult;
                    }
                }

                if (rulesResult.IsError && rulesResult.EndingPosition > koEndingPosition)
                {
                    koEndingPosition = rulesResult.EndingPosition;
                    maxKo = rulesResult;
                }
            }

            if (hasOk)
            {
                max = maxOk;
            }
            else
            {
                max = maxKo;
            }

            result.Root = max.Root;
            result.EndingPosition = max.EndingPosition;
            result.IsError = max.IsError;
            result.IsEnded = max.IsEnded;
            result.HasByPassNodes = max.HasByPassNodes;
            if (hasOk)
            {
                result.AddErrors(max.GetErrors().ToArray());
                var errorCandidates = rulesResults
                    .Where(r => r.IsError)
                    .SelectMany(r => r.GetErrors())
                    .ToList();
                if (errorCandidates.Any())
                {
                    var maxErrorPosition = errorCandidates.Max(e => e.UnexpectedToken.PositionInTokenFlow);
                    var maxErrors = errorCandidates
                        .Where(e => e.UnexpectedToken.PositionInTokenFlow == maxErrorPosition)
                        .ToList();
                    result.AddErrors(maxErrors);
                }
            }
            else if (koEndingPosition >= 0)
            {
                var maxKos = rulesResults
                    .Where(r => r.IsError && r.EndingPosition == koEndingPosition)
                    .ToList();
                foreach (var ko in maxKos)
                {
                    result.AddErrors(ko.GetErrors().ToArray());
                }
            }
            else
            {
                result.AddErrors(max.GetErrors().ToArray());
            }
        }

        parsingContext.Memoize(new NonTerminalClause<IN, OUT>(nonTerminalName), currentPosition, result);
        return result;
    }

    #endregion
}