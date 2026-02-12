using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.tree;
using sly.parser.syntax.grammar;
using sly.parser.llparser.bnf;

namespace sly.parser.llparser.ebnf
{
    public partial  class EBNFRecursiveDescentSyntaxParser<IN, OUT> : RecursiveDescentSyntaxParser<IN, OUT> where IN : struct, Enum
    {
        public EBNFRecursiveDescentSyntaxParser(ParserConfiguration<IN, OUT> configuration, string startingNonTerminal, string i18n)
            : base(configuration, startingNonTerminal, i18n)
        {
        }


        #region parsing

        public override SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, Rule<IN, OUT> rule, int position,
            string nonTerminalName, SyntaxParsingContext<IN, OUT> parsingContext)
        {
            if (rule.IsInfixExpressionRule && rule.IsExpressionRule)
            {
                return ParseInfixExpressionRule(tokens, rule, position, nonTerminalName, parsingContext);
            }

            if (Configuration.CaptureAmbiguities)
            {
                return ParseWithAmbiguities(tokens, rule, position, nonTerminalName, parsingContext);
            }
            
            var currentPosition = position;
            var furthestPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN, OUT>>();
            List<ISyntaxNode<IN, OUT>> choiceAlternatives = null;
            int choiceChildIndex = -1;
            List<AmbiguityInfo<IN, OUT>> choiceAmbiguities = null;
            if (rule.Match(tokens, position, Configuration) && rule.Clauses != null && rule.Clauses.Count > 0)
            {
                children = new List<ISyntaxNode<IN, OUT>>();
                foreach (var clause in rule.Clauses)
                {
                    switch (clause)
                    {
                        case TerminalClause<IN, OUT> termClause:
                        {
                            var termRes =
                                ParseTerminal(tokens, termClause, currentPosition, parsingContext);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            else
                            {
                                var tok = tokens[currentPosition];
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok, LexemeLabels, I18n,
                                    termClause.ExpectedToken));
                            }
                            isError = isError || termRes.IsError;
                            furthestPosition = Math.Max(furthestPosition, termRes.EndingPosition);
                            break;
                        }
                        case NonTerminalClause<IN, OUT> nonTerminalClause:
                        {
                            var nonTerminalResult =
                                ParseNonTerminal(tokens, nonTerminalClause, currentPosition, parsingContext);
                            if (!nonTerminalResult.IsError)
                            {
                                errors.AddRange(nonTerminalResult.GetErrors());
                                children.Add(nonTerminalResult.Root);
                                currentPosition = nonTerminalResult.EndingPosition;
                            }
                            else
                            {
                                errors.AddRange(nonTerminalResult.GetErrors());
                            }
                            isError = isError || nonTerminalResult.IsError;
                            furthestPosition = Math.Max(furthestPosition, nonTerminalResult.EndingPosition);
                            break;
                        }
                        case OneOrMoreClause<IN, OUT> _:
                        case ZeroOrMoreClause<IN, OUT> _:
                        case RepeatClause<IN, OUT> _:
                        {
                            SyntaxParseResult<IN, OUT> manyResult = null;
                            switch (clause)
                            {
                                case RepeatClause<IN,OUT> repeat:
                                    manyResult = ParseRepeat(tokens, repeat, currentPosition, parsingContext);
                                    break;
                                case OneOrMoreClause<IN, OUT> oneOrMore:
                                    manyResult = ParseOneOrMore(tokens, oneOrMore, currentPosition, parsingContext);
                                    break;
                                case ZeroOrMoreClause<IN, OUT> zeroOrMore:
                                    manyResult = ParseZeroOrMore(tokens, zeroOrMore, currentPosition, parsingContext);
                                    break;
                            }

                            if (!manyResult.IsError)
                            {
                                errors.AddRange(manyResult.GetErrors());
                                children.Add(manyResult.Root);
                                currentPosition = manyResult.EndingPosition;
                            }
                            else
                            {
                                if (manyResult.GetErrors() != null && manyResult.GetErrors().Count > 0)
                                    errors.AddRange(manyResult.GetErrors());
                            }
                            isError = manyResult.IsError;
                            furthestPosition = Math.Max(furthestPosition, manyResult.EndingPosition);
                            break;
                        }
                        case OptionClause<IN, OUT> option:
                        {
                            var optionResult = ParseOption(tokens, option, rule, currentPosition, parsingContext);
                            currentPosition = optionResult.EndingPosition;
                            children.Add(optionResult.Root);
                            furthestPosition = Math.Max(furthestPosition, optionResult.EndingPosition);
                            break;
                        }
                        case ChoiceClause<IN, OUT> choice:
                        {
                            var choiceResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                            currentPosition = choiceResult.EndingPosition;
                            if (choiceResult.IsError && choiceResult.GetErrors() != null && choiceResult.GetErrors().Any())
                            {
                                errors.AddRange(choiceResult.GetErrors());
                            }
                            isError = choiceResult.IsError;
                            if (Configuration.CaptureAmbiguities && choiceResult.HasAmbiguity)
                            {
                                choiceAlternatives = choiceResult.AlternativeRoots;
                                choiceChildIndex = children.Count;
                                if (choiceResult.Ambiguities != null && choiceResult.Ambiguities.Any())
                                {
                                    choiceAmbiguities ??= new List<AmbiguityInfo<IN, OUT>>();
                                    choiceAmbiguities.AddRange(choiceResult.Ambiguities);
                                }
                            }
                            children.Add(choiceResult.Root);
                            furthestPosition = Math.Max(furthestPosition, choiceResult.EndingPosition);
                            break;
                        }
                    }

                    if (isError) break;
                }
            }

            var result = new SyntaxParseResult<IN, OUT>();
            result.IsError = isError;
            result.AddErrors(errors);
            result.EndingPosition = furthestPosition;
            if (!isError)
            {
                SyntaxNode<IN, OUT> node = null;
                if (rule.IsSubRule)
                {
                    node = new GroupSyntaxNode<IN, OUT>(nonTerminalName, children);
                    node = ManageExpressionRules(rule, node);
                    result.Root = node;
                    result.IsEnded = currentPosition >= tokens.Length - 1
                                     || currentPosition == tokens.Length - 2 &&
                                     tokens[tokens.Length - 1].IsEOS;
                }
                else
                {
                    if (rule.SubNodeNames != null && rule.SubNodeNames.Length > 0)
                    {
                        for (int i = 0; i < Math.Min(rule.SubNodeNames.Length,children.Count); i++)
                        {
                            var subNodeName = rule.SubNodeNames[i];
                            if (subNodeName != null)
                            {
                                var child = children[i];
                                child.ForceName(subNodeName);
                            }
                        }
                    }
                    node = new SyntaxNode<IN, OUT>( nonTerminalName,  children);
                    node.ForcedName = rule.ForcedName;
                    node.Name = string.IsNullOrEmpty(rule.NodeName) ? nonTerminalName : rule.NodeName;
                    node.ExpressionAffix = rule.ExpressionAffix;
                    node = ManageExpressionRules(rule, node);
                    result.Root = node;
                    result.IsEnded = tokens[result.EndingPosition].IsEOS
                                     || node.IsEpsilon && tokens[result.EndingPosition+1].IsEOS;  
                }

                if (Configuration.CaptureAmbiguities && choiceAlternatives != null && choiceAlternatives.Count > 1)
                {
                    var alternatives = new List<ISyntaxNode<IN, OUT>>();
                    foreach (var alternative in choiceAlternatives)
                    {
                        var altChildren = new List<ISyntaxNode<IN, OUT>>(children);
                        if (choiceChildIndex >= 0 && choiceChildIndex < altChildren.Count)
                        {
                            altChildren[choiceChildIndex] = alternative;
                        }

                        SyntaxNode<IN, OUT> altNode;
                        if (rule.IsSubRule)
                        {
                            altNode = new GroupSyntaxNode<IN, OUT>(nonTerminalName, altChildren);
                            altNode = ManageExpressionRules(rule, altNode);
                        }
                        else
                        {
                            altNode = new SyntaxNode<IN, OUT>(nonTerminalName, altChildren);
                            altNode.ForcedName = rule.ForcedName;
                            altNode.Name = string.IsNullOrEmpty(rule.NodeName) ? nonTerminalName : rule.NodeName;
                            altNode.ExpressionAffix = rule.ExpressionAffix;
                            altNode = ManageExpressionRules(rule, altNode);
                        }

                        alternatives.Add(altNode);
                    }

                    result.AlternativeRoots = alternatives;
                    result.Ambiguities = choiceAmbiguities;
                }
            }

            return result;
        }

        private SyntaxParseResult<IN, OUT> ParseWithAmbiguities(Token<IN>[] tokens, Rule<IN, OUT> rule, int position,
            string nonTerminalName, SyntaxParsingContext<IN, OUT> parsingContext)
        {
            var furthestPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var ambiguities = new List<AmbiguityInfo<IN, OUT>>();
            var paths = new List<(List<ISyntaxNode<IN, OUT>> children, int position, bool hasByPassNodes)>
            {
                (new List<ISyntaxNode<IN, OUT>>(), position, false)
            };

            if (rule.Match(tokens, position, Configuration) && rule.Clauses != null && rule.Clauses.Count > 0)
            {
                foreach (var clause in rule.Clauses)
                {
                    var newPaths = new List<(List<ISyntaxNode<IN, OUT>> children, int position, bool hasByPassNodes)>();
                    foreach (var path in paths)
                    {
                        SyntaxParseResult<IN, OUT> clauseRes = null;
                        switch (clause)
                        {
                            case TerminalClause<IN, OUT> termClause:
                                clauseRes = ParseTerminal(tokens, termClause, path.position, parsingContext);
                                break;
                            case NonTerminalClause<IN, OUT> nonTerminalClause:
                                clauseRes = ParseNonTerminal(tokens, nonTerminalClause, path.position, parsingContext);
                                break;
                            case OneOrMoreClause<IN, OUT> oneOrMore:
                                clauseRes = ParseOneOrMore(tokens, oneOrMore, path.position, parsingContext);
                                break;
                            case ZeroOrMoreClause<IN, OUT> zeroOrMore:
                                clauseRes = ParseZeroOrMore(tokens, zeroOrMore, path.position, parsingContext);
                                break;
                            case RepeatClause<IN, OUT> repeat:
                                clauseRes = ParseRepeat(tokens, repeat, path.position, parsingContext);
                                break;
                            case OptionClause<IN, OUT> option:
                                clauseRes = ParseOption(tokens, option, rule, path.position, parsingContext);
                                break;
                            case ChoiceClause<IN, OUT> choice:
                                clauseRes = ParseChoice(tokens, choice, path.position, parsingContext);
                                break;
                        }

                        if (clauseRes != null && !clauseRes.IsError)
                        {
                            if (clauseRes.GetErrors() != null && clauseRes.GetErrors().Count > 0)
                            {
                                errors.AddRange(clauseRes.GetErrors());
                            }

                            var resultsToProcess = new List<SyntaxParseResult<IN, OUT>>();
                            if (clauseRes.AllResults != null && clauseRes.AllResults.Count > 0)
                            {
                                resultsToProcess.AddRange(clauseRes.AllResults);
                            }
                            else
                            {
                                resultsToProcess.Add(clauseRes);
                            }

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
                        furthestPosition = Math.Max(furthestPosition, newPaths.Max(p => p.position));
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
                    var res = new SyntaxParseResult<IN, OUT>
                    {
                        EndingPosition = pos,
                        IsError = false,
                        Ambiguities = ambiguities,
                        AlternativeRoots = new List<ISyntaxNode<IN, OUT>>(),
                        HasByPassNodes = group.Any(p => p.hasByPassNodes)
                    };

                    foreach (var path in group)
                    {
                        var namedChildren = new List<ISyntaxNode<IN, OUT>>(path.children);
                        if (rule.SubNodeNames != null && rule.SubNodeNames.Length > 0)
                        {
                            for (int i = 0; i < Math.Min(rule.SubNodeNames.Length, namedChildren.Count); i++)
                            {
                                var subNodeName = rule.SubNodeNames[i];
                                if (subNodeName != null)
                                {
                                    namedChildren[i].ForceName(subNodeName);
                                }
                            }
                        }

                        SyntaxNode<IN, OUT> node;
                        if (rule.IsSubRule)
                        {
                            node = new GroupSyntaxNode<IN, OUT>(nonTerminalName, namedChildren);
                            node = ManageExpressionRules(rule, node);
                        }
                        else
                        {
                            node = new SyntaxNode<IN, OUT>(nonTerminalName, namedChildren);
                            node.ForcedName = rule.ForcedName;
                            node.Name = string.IsNullOrEmpty(rule.NodeName) ? nonTerminalName : rule.NodeName;
                            node.ExpressionAffix = rule.ExpressionAffix;
                            node = ManageExpressionRules(rule, node);
                        }

                        if (node.IsByPassNode)
                        {
                            res.AlternativeRoots.Add(namedChildren[0]);
                        }
                        else
                        {
                            res.AlternativeRoots.Add(node);
                        }

                        bool isEnded;
                        if (pos >= tokens.Length)
                        {
                            isEnded = true;
                        }
                        else if (rule.IsSubRule)
                        {
                            isEnded = pos >= tokens.Length - 1
                                      || (pos == tokens.Length - 2 && tokens[tokens.Length - 1].IsEOS);
                        }
                        else
                        {
                            var hasNext = pos + 1 < tokens.Length;
                            isEnded = tokens[pos].IsEOS || (node.IsEpsilon && hasNext && tokens[pos + 1].IsEOS);
                        }

                        res.IsEnded = res.IsEnded || isEnded;
                    }

                    results.Add(res);
                }

                if (results.Count > 0)
                {
                    var best = results.OrderByDescending(r => r.EndingPosition).First();
                    result.AlternativeRoots = best.AlternativeRoots;
                    result.EndingPosition = best.EndingPosition;
                    result.IsEnded = best.IsEnded;
                    result.HasByPassNodes = best.HasByPassNodes;
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

        #endregion
    }
}