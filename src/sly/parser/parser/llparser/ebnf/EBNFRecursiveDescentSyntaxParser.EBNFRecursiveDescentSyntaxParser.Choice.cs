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

        List<SyntaxParseResult<IN, OUT>> alternateResults = new List<SyntaxParseResult<IN, OUT>>();

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

            result = alternateResults.Last();
            if (result.IsOk)
            {
                if (clause.IsTerminalChoice && clause.IsDiscarded && result.Root is SyntaxLeaf<IN, OUT> leaf)
                {
                    var discardedToken = new SyntaxLeaf<IN, OUT>(leaf.Token, true);
                    result.Root = discardedToken;
                }

                parsingContext.Memoize(clause, position, result);
                return result;
            }
        }

        // here all alternateResult ar KO
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
            var errors=  alternateResults.Where(x => x.EndingPosition == greaterPosition).SelectMany(x => x.GetErrors()).ToList();
            result.AddErrors(errors);
            result.IsError = true;
        }

        parsingContext.Memoize(clause, position, result);
        return result;
    }

    #endregion
}