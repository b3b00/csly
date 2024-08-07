using System.Collections.Generic;
using sly.lexer;
using sly.parser;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.ebnf;

public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT>
{
    #region parsing

    public virtual SyntaxParseResult<IN, OUT> ParseInfixExpressionRule(IList<Token<IN>> tokens, Rule<IN,OUT> rule,
        int position,
        string nonTerminalName, SyntaxParsingContext<IN,OUT> parsingContext)
    {
        var currentPosition = position;
        var errors = new List<UnexpectedTokenSyntaxError<IN>>();
        var isError = false;
        var children = new List<ISyntaxNode<IN, OUT>>();
        if (!tokens[position].IsEOS && rule.Match(tokens, position, Configuration) && rule.Clauses != null &&
            rule.Clauses.Count > 0 && MatchExpressionRuleScheme(rule))
        {
            var first = rule.Clauses[0];
            SyntaxParseResult<IN, OUT> firstResult = null;
            if (first is NonTerminalClause<IN,OUT> firstNonTerminal)
            {
                firstResult = ParseNonTerminal(tokens, firstNonTerminal, currentPosition, parsingContext);

                if (firstResult.IsError)
                {
                    return firstResult;
                }
            }

            currentPosition = firstResult.EndingPosition;
            var second = rule.Clauses[1];
            SyntaxParseResult<IN, OUT> secondResult = null;
            switch (second)
            {
                case ChoiceClause<IN,OUT> secondChoice:
                {
                    secondResult = ParseChoice(tokens, secondChoice, currentPosition, parsingContext);

                    if (secondResult.IsError)
                    {
                        if (firstResult.Root is SyntaxNode<IN, OUT>)
                        {
                            firstResult.Errors.AddRange(secondResult.Errors);
                            firstResult.AddExpectings(secondResult.Expecting);
                            return firstResult;
                        }
                    }
                    else
                    {
                        currentPosition = secondResult.EndingPosition;
                    }

                    break;
                }
                case TerminalClause<IN,OUT> secondTerminal:
                {
                    secondResult = ParseTerminal(tokens, secondTerminal, currentPosition, parsingContext);

                    if (secondResult.IsError)
                    {
                        if (firstResult.Root is SyntaxNode<IN, OUT>)
                        {
                            firstResult.Errors.AddRange(secondResult.Errors);
                            firstResult.AddExpectings(secondResult.Expecting);
                            return firstResult;
                        }
                    }

                    break;
                }
            }


            currentPosition = secondResult.EndingPosition;
            var third = rule.Clauses[2];
            SyntaxParseResult<IN, OUT> thirdResult;
            if (third is NonTerminalClause<IN,OUT> thirdNonTerminal)
            {
                thirdResult = ParseNonTerminal(tokens, thirdNonTerminal, currentPosition, parsingContext);
                if (thirdResult.IsError)
                {
                    return thirdResult;
                }
                else
                {
                    children = new List<ISyntaxNode<IN, OUT>>();
                    children.Add(firstResult.Root);
                    children.Add(secondResult.Root);
                    children.Add(thirdResult.Root);
                    currentPosition = thirdResult.EndingPosition;
                    var finalNode = new SyntaxNode<IN, OUT>(nonTerminalName, children);
                    finalNode.ExpressionAffix = rule.ExpressionAffix;
                    finalNode = ManageExpressionRules(rule, finalNode);
                    var finalResult = new SyntaxParseResult<IN, OUT>();
                    finalResult.Root = finalNode;
                    finalResult.IsEnded = currentPosition >= tokens.Count - 1
                                          || currentPosition == tokens.Count - 2 &&
                                          tokens[tokens.Count - 1].IsEOS;
                    finalResult.EndingPosition = currentPosition;
                    return finalResult;
                }
            }
        }


        var result = new SyntaxParseResult<IN, OUT>();
        result.IsError = false;
        result.Errors = errors;
        result.EndingPosition = currentPosition;

        SyntaxNode<IN, OUT> node = null;
        if (rule.IsSubRule)
            node = new GroupSyntaxNode<IN, OUT>(nonTerminalName, children);
        else
            node = new SyntaxNode<IN, OUT>(nonTerminalName, children);
        node = ManageExpressionRules(rule, node);
        if (node.IsByPassNode) // inutile de créer un niveau supplémentaire
            result.Root = children[0];
        result.Root = node;
        result.IsEnded = result.EndingPosition >= tokens.Count - 1
                         || result.EndingPosition == tokens.Count - 2 &&
                         tokens[tokens.Count - 1].IsEOS;
        return result;
    }

    private static bool MatchExpressionRuleScheme(Rule<IN,OUT> rule)
    {
        return rule.Clauses.Count == 3
               && rule.Clauses[0] is NonTerminalClause<IN,OUT>
               && (rule.Clauses[1] is ChoiceClause<IN,OUT> ||
                   rule.Clauses[1] is TerminalClause<IN,OUT>)
               && rule.Clauses[2] is NonTerminalClause<IN,OUT>;
    }

    #endregion
}