using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.ebnf;

public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT> where IN : struct
{
    #region parsing

    public SyntaxParseResult<IN> ParseOption(IList<Token<IN>> tokens, OptionClause<IN,OUT> clause, Rule<IN,OUT> rule,
        int position, SyntaxParsingContext<IN,OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
        {
            return parseResult;
        }

        var result = new SyntaxParseResult<IN>();
        var currentPosition = position;
        var innerClause = clause.Clause;

        SyntaxParseResult<IN> innerResult = null;

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
                    result = new SyntaxParseResult<IN>();
                    result.IsError = true;
                    result.Root = new SyntaxLeaf<IN>(Token<IN>.Empty(), false);
                    result.EndingPosition = position;
                    break;
                case ChoiceClause<IN,OUT> choiceClause:
                {
                    if (choiceClause.IsTerminalChoice)
                    {
                        result = new SyntaxParseResult<IN>();
                        result.IsError = false;
                        result.Root = new SyntaxLeaf<IN>(Token<IN>.Empty(), false);
                        result.EndingPosition = position;
                    }
                    else if (choiceClause.IsNonTerminalChoice)
                    {
                        result = new SyntaxParseResult<IN>();
                        result.IsError = false;
                        result.Root = new SyntaxEpsilon<IN>();
                        result.EndingPosition = position;
                    }

                    break;
                }
                default:
                {
                    result = new SyntaxParseResult<IN>();
                    result.IsError = true;
                    var children = new List<ISyntaxNode<IN>> { innerResult.Root };
                    if (innerResult.IsError) children.Clear();
                    result.Root = new OptionSyntaxNode<IN>(rule.NonTerminalName, children,
                        rule.GetVisitor());
                    (result.Root as OptionSyntaxNode<IN>).IsGroupOption = clause.IsGroupOption;
                    result.EndingPosition = position;
                    break;
                }
            }
        }
        else
        {
            var children = new List<ISyntaxNode<IN>> { innerResult.Root };
            result.Root =
                new OptionSyntaxNode<IN>(rule.NonTerminalName, children, rule.GetVisitor());
            result.EndingPosition = innerResult.EndingPosition;
            result.HasByPassNodes = innerResult.HasByPassNodes;
        }

        parsingContext.Memoize(clause, position, result);
        return result;
    }

    #endregion
}