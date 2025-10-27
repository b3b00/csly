using System.Collections.Generic;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.ebnf;

public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT>
{
    #region parsing

    public virtual SyntaxParseResult<IN, OUT> ParseInfixExpressionRule(Token<IN>[] tokens, Rule<IN, OUT> rule,
        int position,
        string nonTerminalName, SyntaxParsingContext<IN, OUT> parsingContext)
    {
        var currentPosition = position;
        var children = new List<ISyntaxNode<IN, OUT>>(3); // Optimization: Pre-allocate for 3 children
        
        // Optimization: Early exit checks
        if (tokens[position].IsEOS || !rule.Match(tokens, position, Configuration) || 
            rule.Clauses == null || rule.Clauses.Count == 0 || !MatchExpressionRuleScheme(rule))
        {
            return CreateDefaultExpressionResult(rule, nonTerminalName, currentPosition, tokens, children);
        }
        
        var first = rule.Clauses[0];
        SyntaxParseResult<IN, OUT> firstResult = null;
        if (first is NonTerminalClause<IN, OUT> firstNonTerminal)
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
            case ChoiceClause<IN, OUT> secondChoice:
            {
                secondResult = ParseChoice(tokens, secondChoice, currentPosition, parsingContext);

                if (secondResult.IsError)
                {
                    if (firstResult.Root is SyntaxNode<IN, OUT>)
                    {
                        firstResult.AddErrors(secondResult.GetErrors());
                        return firstResult;
                    }
                }
                else
                {
                    currentPosition = secondResult.EndingPosition;
                }

                break;
            }
            case TerminalClause<IN, OUT> secondTerminal:
            {
                secondResult = ParseTerminal(tokens, secondTerminal, currentPosition, parsingContext);

                if (secondResult.IsError)
                {
                    if (firstResult.Root is SyntaxNode<IN, OUT>)
                    {
                        firstResult.AddErrors(secondResult.GetErrors());
                        return firstResult;
                    }
                }

                break;
            }
        }


        currentPosition = secondResult.EndingPosition;
        var third = rule.Clauses[2];
        SyntaxParseResult<IN, OUT> thirdResult;
        if (third is NonTerminalClause<IN, OUT> thirdNonTerminal)
        {
            thirdResult = ParseNonTerminal(tokens, thirdNonTerminal, currentPosition, parsingContext);
            if (thirdResult.IsError)
            {
                return thirdResult;
            }
            
            // Optimization: Build result inline
            children.Add(firstResult.Root);
            children.Add(secondResult.Root);
            children.Add(thirdResult.Root);
            currentPosition = thirdResult.EndingPosition;
            
            var finalNode = new SyntaxNode<IN, OUT>(rule.NodeName ?? nonTerminalName, children);
            finalNode.ExpressionAffix = rule.ExpressionAffix;
            finalNode = ManageExpressionRules(rule, finalNode);
            
            var finalResult = new SyntaxParseResult<IN, OUT>
            {
                Root = finalNode,
                IsEnded = currentPosition >= tokens.Length - 1 || 
                          (currentPosition == tokens.Length - 2 && tokens[tokens.Length - 1].IsEOS),
                EndingPosition = currentPosition
            };
            return finalResult;
        }

        return CreateDefaultExpressionResult(rule, nonTerminalName, currentPosition, tokens, children);
    }
    
    // Optimization: Extract method to avoid code duplication
    private SyntaxParseResult<IN, OUT> CreateDefaultExpressionResult(Rule<IN, OUT> rule, string nonTerminalName, 
        int currentPosition, Token<IN>[] tokens, List<ISyntaxNode<IN, OUT>> children)
    {
        var result = new SyntaxParseResult<IN, OUT>
        {
            IsError = false,
            EndingPosition = currentPosition
        };

        SyntaxNode<IN, OUT> node = rule.IsSubRule
            ? new GroupSyntaxNode<IN, OUT>(nonTerminalName, children)
            : new SyntaxNode<IN, OUT>(nonTerminalName, children);
        
        node = ManageExpressionRules(rule, node);
        result.Root = node.IsByPassNode && children.Count > 0 ? children[0] : node;
        result.IsEnded = result.EndingPosition >= tokens.Length - 1 ||
                         (result.EndingPosition == tokens.Length - 2 && tokens[tokens.Length - 1].IsEOS);
        return result;
    }

    private static bool MatchExpressionRuleScheme(Rule<IN, OUT> rule)
    {
        return rule.Clauses.Count == 3
               && rule.Clauses[0] is NonTerminalClause<IN, OUT>
               && (rule.Clauses[1] is ChoiceClause<IN, OUT> ||
                   rule.Clauses[1] is TerminalClause<IN, OUT>)
               && rule.Clauses[2] is NonTerminalClause<IN, OUT>;
    }

    #endregion
}