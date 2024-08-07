using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.ebnf;

public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT> where IN : struct
{
    #region parsing

    public SyntaxParseResult<IN, OUT> ParseOption(IList<Token<IN>> tokens, OptionClause<IN,OUT> clause, Rule<IN,OUT> rule,
        int position, SyntaxParsingContext<IN,OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
        {
            return parseResult;
        }

        var result = new SyntaxParseResult<IN, OUT>();
        var currentPosition = position;
        var innerClause = clause.Clause;

        SyntaxParseResult<IN, OUT> innerResult = null;

        switch (innerClause)
        {
            case TerminalClause<IN,OUT> term:
                innerResult = ParseTerminal(tokens, term, currentPosition, parsingContext);
                break;
            case NonTerminalClause<IN,OUT> nonTerm:
                innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                break;
            case ChoiceClause<IN,OUT> choice:
                innerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                break;
            default:
                throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
        }


        if (innerResult.IsError)
        {
            switch (innerClause)
            {
                case TerminalClause<IN,OUT> _:
                    result = new SyntaxParseResult<IN, OUT>();
                    result.IsError = true;
                    result.Root = new SyntaxLeaf<IN, OUT>(Token<IN>.Empty(), false);
                    result.EndingPosition = position;
                    break;
                case ChoiceClause<IN,OUT> choiceClause:
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
                        result = new SyntaxParseResult<IN, OUT>();
                        result.IsError = false;
                        result.Root = new SyntaxEpsilon<IN, OUT>();
                        result.EndingPosition = position;
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
                        rule.GetVisitor());
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
                new OptionSyntaxNode<IN, OUT>(rule.NonTerminalName, children, rule.GetVisitor());
            result.EndingPosition = innerResult.EndingPosition;
            result.HasByPassNodes = innerResult.HasByPassNodes;
        }

        parsingContext.Memoize(clause, position, result);
        return result;
    }

    #endregion
}