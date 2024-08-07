using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.ebnf;

public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT> where IN : struct
{
    #region parsing

    public SyntaxParseResult<IN, OUT> ParseChoice(IList<Token<IN>> tokens, ChoiceClause<IN,OUT> clause,
        int position, SyntaxParsingContext<IN,OUT> parsingContext)
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


        foreach (var alternate in clause.Choices)
        {
            switch (alternate)
            {
                case TerminalClause<IN,OUT> terminalAlternate:
                    result = ParseTerminal(tokens, terminalAlternate, currentPosition, parsingContext);
                    break;
                case NonTerminalClause<IN,OUT> nonTerminalAlternate:
                    result = ParseNonTerminal(tokens, nonTerminalAlternate, currentPosition, parsingContext);
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater inside  " + clause.GetType().Name);
            }

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

        if (result.IsError && clause.IsTerminalChoice)
        {
            var terminalAlternates = clause.Choices.Cast<TerminalClause<IN,OUT>>();
            var expected = terminalAlternates.Select(x => x.ExpectedToken).ToList();
            result.Errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], LexemeLabels, I18n,
                expected.ToArray()));
        }

        parsingContext.Memoize(clause, position, result);
        return result;
    }

    #endregion
}