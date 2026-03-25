using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;
using System.Linq;
using System;

namespace sly.parser.llparser.bnf
{
    public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct, Enum
    {
        public RecursiveDescentSyntaxParser(ParserConfiguration<IN, OUT> configuration, string startingNonTerminal,
            string i18n)
        {

            I18n = i18n;
            Configuration = configuration;
            StartingNonTerminal = startingNonTerminal;
            ComputeSubRules(configuration);
            InitializeStartingTokens(Configuration, startingNonTerminal);
        }

        public Dictionary<IN, Dictionary<string, string>> LexemeLabels { get; set; }

        #region parsing

        public SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, string startingNonTerminal = null)
        {
            return SafeParse(tokens, new SyntaxParsingContext<IN, OUT>(Configuration.UseMemoization),
                startingNonTerminal);
        }

        public SyntaxParseResult<IN, OUT> SafeParse(Token<IN>[] tokens, SyntaxParsingContext<IN, OUT> parsingContext,
            string startingNonTerminal = null)
        {
            var start = startingNonTerminal ?? StartingNonTerminal;
            var NonTerminals = Configuration.NonTerminals;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var nt = NonTerminals[start];

            var rs = new List<SyntaxParseResult<IN, OUT>>();

            var matchingRuleCount = 0;

            foreach (var rule in nt.Rules)
            {
                if (!tokens[0].IsEOS && rule.Match(tokens, 0, Configuration)
                    || tokens[0].IsEOS && rule.MayBeEmpty)
                {
                    matchingRuleCount++;
                    var r = Parse(tokens, rule, 0, start, parsingContext);
                    rs.Add(r);
                }
            }

            if (matchingRuleCount == 0)
            {
                errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[0], LexemeLabels, I18n,
                    nt.GetPossibleLeadingTokens().ToArray()));
            }

            SyntaxParseResult<IN, OUT> result = null;


            if (rs.Count > 0)
            {
                // Check for ambiguities at root level
                var successfulResults = rs.SelectMany(r => r.AllResults ?? new List<SyntaxParseResult<IN, OUT>> { r })
                    .Where(r => r.IsEnded && !r.IsError).ToList();


                if (Configuration.CaptureAmbiguities &&
                    (successfulResults.Count > 1 || successfulResults.Any(r => r.HasAmbiguity)))
                {
                    // Ambiguity detected at root level
                    result = new SyntaxParseResult<IN, OUT>
                    {
                        AlternativeRoots = successfulResults.SelectMany(r => r.AlternativeRoots).ToList(),
                        EndingPosition = successfulResults.Max(r => r.EndingPosition),
                        IsError = false,
                        IsEnded = true,
                        HasByPassNodes = successfulResults.Any(r => r.HasByPassNodes)
                    };

                    if (result.Ambiguities == null)
                        result.Ambiguities = new List<AmbiguityInfo<IN, OUT>>();

                    if (successfulResults.Count > 1)
                    {
                        result.Ambiguities.Add(new AmbiguityInfo<IN, OUT>
                        {
                            NonTerminalName = start,
                            Position = 0,
                            AlternativeCount = successfulResults.Count
                        });
                    }

                    foreach (var success in successfulResults)
                    {
                        if (success.Ambiguities != null)
                        {
                            result.Ambiguities.AddRange(success.Ambiguities);
                        }
                    }
                }
                else if (successfulResults.Count > 0)
                {
                    result = successfulResults[0];
                }
                else
                {
                    // No successful results, try to find any completed parse
                    result = rs.FirstOrDefault(r => r.IsEnded);
                    if (result == null)
                    {
                        result = rs.FirstOrDefault();
                    }
                }


                if (result == null)
                {
                    int lastPosition = -1;
                    List<SyntaxParseResult<IN, OUT>> furtherResults = new List<SyntaxParseResult<IN, OUT>>();
                    foreach (var r in rs)
                    {
                        if (r.EndingPosition > lastPosition)
                        {
                            lastPosition = r.EndingPosition;
                            furtherResults.Clear();
                            errors.Clear();
                        }

                        if (r.EndingPosition == lastPosition)
                        {
                            furtherResults.Add(r);
                            errors.AddRange(r.GetErrors());
                        }
                    }


                    if (errors.Count == 0)
                    {
                        errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[lastPosition], LexemeLabels, null));
                    }
                }
                else
                {
                    if (result.EndingPosition < tokens.Length - 1)
                    {
                        SyntaxParseResult<IN, OUT> r = new SyntaxParseResult<IN, OUT>()
                        {
                            IsError = true,
                            IsEnded = false,
                            EndingPosition = result.EndingPosition
                        };
                        var allErrors = rs.SelectMany(x => x.GetErrors() ?? new List<UnexpectedTokenSyntaxError<IN>>())
                            .ToList();
                        if (allErrors.Any())
                        {
                            var maxPosition = allErrors.Max(e => e.UnexpectedToken.PositionInTokenFlow);
                            var maxErrors = allErrors
                                .Where(e => e.UnexpectedToken.PositionInTokenFlow == maxPosition)
                                .ToList();
                            r.AddErrors(maxErrors);
                        }
                        else
                        {
                            r.AddErrors(result.GetErrors().ToArray());
                            if (r.GetErrors() == null || r.GetErrors().Count == 0)
                            {
                                var unexpectedToken = tokens[result.EndingPosition];
                                r.AddError(new UnexpectedTokenSyntaxError<IN>(unexpectedToken, LexemeLabels, I18n,
                                    Array.Empty<LeadingToken<IN>>()));
                            }
                        }

                        return r;
                    }
                }
            }

            if (result == null)
            {
                result = new SyntaxParseResult<IN, OUT>();

                if (errors.Count > 0)
                {
                    var lastErrorPosition = errors
                        .Select(e => e.UnexpectedToken.PositionInTokenFlow)
                        .Max();
                    var lastErrors = errors
                        .Where(e =>
                            e.UnexpectedToken.PositionInTokenFlow == lastErrorPosition)
                        .ToList();
                    result.AddErrors(lastErrors);
                }
                else
                {
                    result.AddErrors(errors);
                }

                result.IsError = true;
            }

            return result;
        }


        public virtual SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, Rule<IN, OUT> rule, int position,
            string nonTerminalName, SyntaxParsingContext<IN, OUT> parsingContext)
        {
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var paths = new List<(List<ISyntaxNode<IN, OUT>> children, int position)>
                { (new List<ISyntaxNode<IN, OUT>>(), position) };
            var ambiguities = new List<AmbiguityInfo<IN, OUT>>();
            var furthestPosition = position;

            if (position < tokens.Length && !tokens[position].IsEOS && rule.Match(tokens, position, Configuration) &&
                rule.Clauses is { Count: > 0 })
            {
                foreach (var clause in rule.Clauses)
                {
                    var newPaths = new List<(List<ISyntaxNode<IN, OUT>> children, int position)>();
                    foreach (var path in paths)
                    {
                        SyntaxParseResult<IN, OUT> clauseRes = null;
                        switch (clause)
                        {
                            case TerminalClause<IN, OUT> terminalClause:
                                clauseRes = ParseTerminal(tokens, terminalClause, path.position, parsingContext);
                                break;
                            case NonTerminalClause<IN, OUT> nonTerminalClause:
                                clauseRes = ParseNonTerminal(tokens, nonTerminalClause, path.position, parsingContext);
                                break;
                        }

                        if (clauseRes != null && !clauseRes.IsError)
                        {
                            if (clauseRes.GetErrors() != null && clauseRes.GetErrors().Count > 0)
                            {
                                errors.AddRange(clauseRes.GetErrors());
                            }

                            var resultsToProcess = new List<SyntaxParseResult<IN, OUT>>();
                            if (Configuration.CaptureAmbiguities && clauseRes.AllResults != null)
                            {
                                resultsToProcess.AddRange(clauseRes.AllResults);
                            }
                            else
                            {
                                resultsToProcess.Add(clauseRes);
                            }

                            foreach (var res in resultsToProcess)
                            {
                                foreach (var alt in res.AlternativeRoots)
                                {
                                    var newCombo = new List<ISyntaxNode<IN, OUT>>(path.children);
                                    newCombo.Add(alt);
                                    newPaths.Add((newCombo, res.EndingPosition));
                                }

                                if (res.Ambiguities != null)
                                {
                                    ambiguities.AddRange(res.Ambiguities);
                                }
                            }
                        }
                        else if (clauseRes != null)
                        {
                            errors.AddRange(clauseRes.GetErrors());
                        }
                    }

                    if (newPaths.Any())
                    {
                        furthestPosition = newPaths.Max(p => p.position);
                    }

                    paths = newPaths;
                    if (paths.Count == 0)
                    {
                        isError = true;
                        break;
                    }
                }
            }
            else
            {
                isError = true;
            }

            var result = new SyntaxParseResult<IN, OUT>();
            result.IsError = isError;
            result.EndingPosition = furthestPosition;
            result.AddErrors(errors);

            if (!isError)
            {
                var resultsByPosition = paths.GroupBy(p => p.position).ToList();
                var results = new List<SyntaxParseResult<IN, OUT>>();

                foreach (var group in resultsByPosition)
                {
                    var pos = group.Key;
                    var res = new SyntaxParseResult<IN, OUT>();
                    res.EndingPosition = pos;
                    res.IsError = false;
                    res.IsEnded = pos >= tokens.Length || tokens[pos].IsEOS;
                    res.Ambiguities = ambiguities;
                    res.AlternativeRoots = new List<ISyntaxNode<IN, OUT>>();
                    foreach (var path in group)
                    {
                        SyntaxNode<IN, OUT> node = null;
                        if (rule.IsSubRule)
                            node = new GroupSyntaxNode<IN, OUT>(nonTerminalName, path.children);
                        else
                            node = new SyntaxNode<IN, OUT>(rule.NodeName ?? nonTerminalName, path.children);
                        node = ManageExpressionRules(rule, node);
                        if (node.IsByPassNode)
                        {
                            res.AlternativeRoots.Add(path.children[0]);
                        }
                        else
                        {
                            res.AlternativeRoots.Add(node);
                        }
                    }

                    results.Add(res);
                }

                if (results.Count > 0)
                {
                    var best = results.OrderByDescending(r => r.EndingPosition).First();
                    result.AlternativeRoots = best.AlternativeRoots;
                    result.EndingPosition = best.EndingPosition;
                    result.IsEnded = best.IsEnded;
                    result.AllResults = results;
                    result.Ambiguities = ambiguities;
                }
                else
                {
                    result.IsError = true;
                }
            }

            return result;
        }




        private SyntaxParseResult<IN, OUT> NoMatchingRuleError(Token<IN>[] tokens, int currentPosition,
            List<LeadingToken<IN>> allAcceptableTokens)
        {
            var noRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();

            if (currentPosition < tokens.Length)
            {
                noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], I18n,
                    allAcceptableTokens));
            }
            else
            {
                noRuleErrors.Add(new UnexpectedTokenSyntaxError<IN>(new Token<IN> { IsEOS = true }, I18n,
                    allAcceptableTokens));
            }

            var error = new SyntaxParseResult<IN, OUT>();
            error.IsError = true;
            error.Root = null;
            error.IsEnded = false;
            error.AddErrors(noRuleErrors);
            error.EndingPosition = currentPosition;
            error.Expecting = allAcceptableTokens;

            return error;
        }




        public virtual void Init(ParserConfiguration<IN, OUT> configuration, string root)
        {
            if (root != null) StartingNonTerminal = root;
            Configuration = configuration;
            // #540 : reset all leading tokens and recompute with the new configuration (expression rules)
            foreach (var nonTerminal in configuration.NonTerminals.Values)
            {
                nonTerminal?.Rules?.ForEach(x => x?.PossibleLeadingTokens?.Clear());
            }

            InitializeStartingTokens(configuration, StartingNonTerminal);
        }



        #endregion
    }
}