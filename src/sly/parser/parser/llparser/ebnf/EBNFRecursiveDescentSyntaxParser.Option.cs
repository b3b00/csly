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

    public SyntaxParseResult<IN, OUT> ParseOption(Token<IN>[] tokens, OptionClause<IN, OUT> clause, Rule<IN, OUT> rule,
        int position, SyntaxParsingContext<IN, OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
        {
            return parseResult;
        }

        if (Configuration.CaptureAmbiguities)
        {
            var innerClauseAmbi = clause.Clause;
            SyntaxParseResult<IN, OUT> innerResultAmbi = null;

            switch (innerClauseAmbi)
            {
                case TerminalClause<IN, OUT> term:
                    innerResultAmbi = ParseTerminal(tokens, term, position, parsingContext);
                    break;
                case NonTerminalClause<IN, OUT> nonTerm:
                    innerResultAmbi = ParseNonTerminal(tokens, nonTerm, position, parsingContext);
                    break;
                case ChoiceClause<IN, OUT> choice:
                    innerResultAmbi = ParseChoice(tokens, choice, position, parsingContext);
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater to " + innerClauseAmbi.GetType().Name);
            }

            var resultAmbi = new SyntaxParseResult<IN, OUT>
            {
                IsError = false,
                EndingPosition = position,
                AlternativeRoots = new List<ISyntaxNode<IN, OUT>>(),
                AllResults = new List<SyntaxParseResult<IN, OUT>>()
            };

            var emptyNode = new OptionSyntaxNode<IN, OUT>(rule.NonTerminalName, new List<ISyntaxNode<IN, OUT>>(),
                rule.GetVisitorMethod())
            {
                IsGroupOption = clause.IsGroupOption
            };

            var emptyResult = new SyntaxParseResult<IN, OUT>
            {
                IsError = false,
                Root = emptyNode,
                EndingPosition = position,
                IsEnded = position >= tokens.Length || (position < tokens.Length && tokens[position].IsEOS)
            };
            resultAmbi.AllResults.Add(emptyResult);

            if (innerResultAmbi != null && !innerResultAmbi.IsError)
            {
                var innerResults = innerResultAmbi.AllResults ?? new List<SyntaxParseResult<IN, OUT>> { innerResultAmbi };
                foreach (var inner in innerResults)
                {
                    var alternatives = inner.AlternativeRoots;
                    if (alternatives == null || alternatives.Count == 0)
                    {
                        alternatives = new List<ISyntaxNode<IN, OUT>> { inner.Root };
                    }

                    foreach (var alt in alternatives)
                    {
                        var optionNode = new OptionSyntaxNode<IN, OUT>(rule.NonTerminalName,
                            new List<ISyntaxNode<IN, OUT>> { alt }, rule.GetVisitorMethod())
                        {
                            IsGroupOption = clause.IsGroupOption
                        };

                        var optionResult = new SyntaxParseResult<IN, OUT>
                        {
                            IsError = false,
                            Root = optionNode,
                            EndingPosition = inner.EndingPosition,
                            IsEnded = inner.IsEnded,
                            HasByPassNodes = inner.HasByPassNodes
                        };
                        resultAmbi.AllResults.Add(optionResult);
                    }
                }

                if (innerResultAmbi.GetErrors() != null)
                {
                    resultAmbi.AddErrors(innerResultAmbi.GetErrors());
                }
            }
            else if (innerResultAmbi != null)
            {
                resultAmbi.AddErrors(innerResultAmbi.GetErrors());
            }

            var best = resultAmbi.AllResults.OrderBy(r => r.EndingPosition).Reverse().FirstOrDefault();
            if (best != null)
            {
                resultAmbi.Root = best.Root;
                resultAmbi.EndingPosition = best.EndingPosition;
                resultAmbi.IsEnded = best.IsEnded;
                resultAmbi.HasByPassNodes = best.HasByPassNodes;
                resultAmbi.AlternativeRoots = best.AlternativeRoots;
            }

            parsingContext.Memoize(clause, position, resultAmbi);
            return resultAmbi;
        }

        var result = new SyntaxParseResult<IN, OUT>();
        var currentPosition = position;
        var innerClause = clause.Clause;

        SyntaxParseResult<IN, OUT> innerResult = null;

        switch (innerClause)
        {
            case TerminalClause<IN, OUT> term:
                innerResult = ParseTerminal(tokens, term, currentPosition, parsingContext);
                break;
            case NonTerminalClause<IN, OUT> nonTerm:
                innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                break;
            case ChoiceClause<IN, OUT> choice:
                innerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                break;
            default:
                throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
        }


        if (innerResult.IsError)
        {
            switch (innerClause)
            {
                case TerminalClause<IN, OUT> _:
                    result = new SyntaxParseResult<IN, OUT>();
                    result.IsError = true;
                    result.Root = new SyntaxLeaf<IN, OUT>(Token<IN>.Empty(), false);
                    result.EndingPosition = position;
                    break;
                case ChoiceClause<IN, OUT> choiceClause:
                {
                    if (choiceClause.IsTerminalChoice)
                    {
                        result = new SyntaxParseResult<IN, OUT>();
                        result.IsError = false;
                        result.Root = new SyntaxLeaf<IN, OUT>(Token<IN>.Empty(), false);
                        result.EndingPosition = position;
                    }
                    else if (choiceClause.IsNonTerminalChoice)
                    {
                        result = new SyntaxParseResult<IN, OUT>
                        {
                            IsError = false,
                            Root = new OptionSyntaxNode<IN, OUT>(rule.NonTerminalName, new List<ISyntaxNode<IN, OUT>>(),
                                rule.GetVisitorMethod()),
                            EndingPosition = position
                        };
                    }

                    break;
                }
                default:
                {
                    result = new SyntaxParseResult<IN, OUT>();
                    result.IsError = true;
                    var children = new List<ISyntaxNode<IN, OUT>> { innerResult.Root };
                    if (innerResult.IsError) children.Clear();
                    result.Root = new OptionSyntaxNode<IN, OUT>(rule.NonTerminalName, children,
                        rule.GetVisitorMethod());
                    (result.Root as OptionSyntaxNode<IN, OUT>).IsGroupOption = clause.IsGroupOption;
                    result.EndingPosition = position;
                    break;
                }
            }
        }
        else
        {
            var children = new List<ISyntaxNode<IN, OUT>> { innerResult.Root };
            result.Root =
                new OptionSyntaxNode<IN, OUT>(rule.NonTerminalName, children, rule.GetVisitorMethod());
            result.EndingPosition = innerResult.EndingPosition;
            result.HasByPassNodes = innerResult.HasByPassNodes;
        }

        parsingContext.Memoize(clause, position, result);
        return result;
    }

    #endregion
}