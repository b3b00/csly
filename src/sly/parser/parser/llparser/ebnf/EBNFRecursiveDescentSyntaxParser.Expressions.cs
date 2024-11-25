using System.Collections.Generic;
using sly.lexer;
using sly.parser;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.ebnf;

public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT>
{
    #region parsing

    public virtual SyntaxParseResult<IN> ParseInfixExpressionRule(IList<Token<IN>> tokens, Rule<IN> rule,
        int position,
        string nonTerminalName, SyntaxParsingContext<IN> parsingContext)
    {
        var currentPosition = position;
        var isError = false;
        var children = new List<ISyntaxNode<IN>>();
        if (!tokens[position].IsEOS && rule.Match(tokens, position, Configuration) && rule.Clauses != null &&
            rule.Clauses.Count > 0 && MatchExpressionRuleScheme(rule))
        {
            var first = rule.Clauses[0];
            SyntaxParseResult<IN> firstResult = null;
            if (first is NonTerminalClause<IN> firstNonTerminal)
            {
                firstResult = ParseNonTerminal(tokens, firstNonTerminal, currentPosition, parsingContext);

                if (firstResult.IsError)
                {
                    return firstResult;
                }
            }

            currentPosition = firstResult.EndingPosition;
            var second = rule.Clauses[1];
            SyntaxParseResult<IN> secondResult = null;
            switch (second)
            {
                case ChoiceClause<IN> secondChoice:
                {
                    secondResult = ParseChoice(tokens, secondChoice, currentPosition, parsingContext);

                    if (secondResult.IsError)
                    {
                        if (firstResult.Root is SyntaxNode<IN>)
                        {
                            firstResult.AddErrors(secondResult.GetErrors());
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
                case TerminalClause<IN> secondTerminal:
                {
                    secondResult = ParseTerminal(tokens, secondTerminal, currentPosition, parsingContext);

                    if (secondResult.IsError)
                    {
                        if (firstResult.Root is SyntaxNode<IN>)
                        {
                            firstResult.AddErrors(secondResult.GetErrors());
                            firstResult.AddExpectings(secondResult.Expecting);
                            return firstResult;
                        }
                    }

                    break;
                }
            }


            currentPosition = secondResult.EndingPosition;
            var third = rule.Clauses[2];
            SyntaxParseResult<IN> thirdResult;
            if (third is NonTerminalClause<IN> thirdNonTerminal)
            {
                thirdResult = ParseNonTerminal(tokens, thirdNonTerminal, currentPosition, parsingContext);
                if (thirdResult.IsError)
                {
                    return thirdResult;
                }
                else
                {
                    children = new List<ISyntaxNode<IN>>();
                    children.Add(firstResult.Root);
                    children.Add(secondResult.Root);
                    children.Add(thirdResult.Root);
                    currentPosition = thirdResult.EndingPosition;
                    var finalNode = new SyntaxNode<IN>(nonTerminalName, children);
                    finalNode.ExpressionAffix = rule.ExpressionAffix;
                    finalNode = ManageExpressionRules(rule, finalNode);
                    var finalResult = new SyntaxParseResult<IN>();
                    finalResult.Root = finalNode;
                    finalResult.IsEnded = currentPosition >= tokens.Count - 1
                                          || currentPosition == tokens.Count - 2 &&
                                          tokens[tokens.Count - 1].IsEOS;
                    finalResult.EndingPosition = currentPosition;
                    return finalResult;
                }
            }
        }


        var result = new SyntaxParseResult<IN>();
        result.IsError = false;
        result.EndingPosition = currentPosition;

        SyntaxNode<IN> node = null;
        if (rule.IsSubRule)
            node = new GroupSyntaxNode<IN>(nonTerminalName, children);
        else
            node = new SyntaxNode<IN>(nonTerminalName, children);
        node = ManageExpressionRules(rule, node);
        if (node.IsByPassNode) // inutile de créer un niveau supplémentaire
            result.Root = children[0];
        result.Root = node;
        result.IsEnded = result.EndingPosition >= tokens.Count - 1
                         || result.EndingPosition == tokens.Count - 2 &&
                         tokens[tokens.Count - 1].IsEOS;
        return result;
    }

    private static bool MatchExpressionRuleScheme(Rule<IN> rule)
    {
        return rule.Clauses.Count == 3
               && rule.Clauses[0] is NonTerminalClause<IN>
               && (rule.Clauses[1] is ChoiceClause<IN> ||
                   rule.Clauses[1] is TerminalClause<IN>)
               && rule.Clauses[2] is NonTerminalClause<IN>;
    }

    #endregion
}