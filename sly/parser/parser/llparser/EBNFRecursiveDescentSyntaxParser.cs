using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.tree;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser
{
    public partial  class EBNFRecursiveDescentSyntaxParser<IN, OUT> : RecursiveDescentSyntaxParser<IN, OUT> where IN : struct
    {
        public EBNFRecursiveDescentSyntaxParser(ParserConfiguration<IN, OUT> configuration, string startingNonTerminal, string i18n)
            : base(configuration, startingNonTerminal, i18n)
        {
        }


        #region parsing

        public override SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, Rule<IN> rule, int position,
            string nonTerminalName)
        {
            
            if (rule.IsInfixExpressionRule && rule.IsExpressionRule)
            {
                return ParseInfixExpressionRule(tokens, rule, position, nonTerminalName);
            }
            
            var currentPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN>>();
            if (rule.PossibleLeadingTokens.Contains(tokens[position].TokenID) || rule.MayBeEmpty)
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<IN>>();
                    foreach (var clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<IN> termClause)
                        {
                            var termRes =
                                ParseTerminal(tokens, termClause, currentPosition);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            else
                            {
                                var tok = tokens[currentPosition];
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok,I18n,
                                    ((TerminalClause<IN>) clause).ExpectedToken));
                            }

                            isError = isError || termRes.IsError;
                        }
                        else if (clause is NonTerminalClause<IN>)
                        {
                            var nonTerminalResult =
                                ParseNonTerminal(tokens, clause as NonTerminalClause<IN>, currentPosition);
                            if (!nonTerminalResult.IsError)
                            {
                                errors.AddRange(nonTerminalResult.Errors);
                                children.Add(nonTerminalResult.Root);
                                currentPosition = nonTerminalResult.EndingPosition;
                            }
                            else
                            {
                                errors.AddRange(nonTerminalResult.Errors);
                            }

                            isError = isError || nonTerminalResult.IsError;
                        }
                        else if (clause is OneOrMoreClause<IN> || clause is ZeroOrMoreClause<IN>)
                        {
                            SyntaxParseResult<IN> manyResult = null;
                            if (clause is OneOrMoreClause<IN> oneOrMore)
                                manyResult = ParseOneOrMore(tokens, oneOrMore, currentPosition);
                            else if (clause is ZeroOrMoreClause<IN> zeroOrMore)
                                manyResult = ParseZeroOrMore(tokens, zeroOrMore, currentPosition);
                            if (!manyResult.IsError)
                            {
                                errors.AddRange(manyResult.Errors);
                                children.Add(manyResult.Root);
                                currentPosition = manyResult.EndingPosition;
                            }
                            else
                            {
                                if (manyResult.Errors != null && manyResult.Errors.Count > 0)
                                    errors.AddRange(manyResult.Errors);
                            }

                            isError = manyResult.IsError;
                        }
                        else if (clause is OptionClause<IN> option)
                        {
                            var optionResult = ParseOption(tokens, option, rule, currentPosition);
                            currentPosition = optionResult.EndingPosition;
                            children.Add(optionResult.Root);
                        }
                        else if (clause is ChoiceClause<IN> choice)
                        {
                            var choiceResult = ParseChoice(tokens, choice, currentPosition);
                            currentPosition = choiceResult.EndingPosition;
                            if (choiceResult.IsError && choiceResult.Errors != null && choiceResult.Errors.Any())
                            {
                                errors.AddRange(choiceResult.Errors);
                            }

                            isError = choiceResult.IsError;

                            children.Add(choiceResult.Root);
                        }

                        if (isError) break;
                    }
                }

            var result = new SyntaxParseResult<IN>();
            result.IsError = isError;
            result.Errors = errors;
            result.EndingPosition = currentPosition;
            if (!isError)
            {
                SyntaxNode<IN> node = null;
                if (rule.IsSubRule)
                {
                    node = new GroupSyntaxNode<IN>(nonTerminalName, children);
                    node = ManageExpressionRules(rule, node);
                    result.Root = node;
                    result.IsEnded = currentPosition >= tokens.Count - 1
                                     || currentPosition == tokens.Count - 2 &&
                                     tokens[tokens.Count - 1].IsEOS;
                }
                else
                {
                    node = new SyntaxNode<IN>( nonTerminalName,  children);
                    node.ExpressionAffix = rule.ExpressionAffix;
                    node = ManageExpressionRules(rule, node);
                    result.Root = node;
                    result.IsEnded = currentPosition >= tokens.Count - 1
                                     || currentPosition == tokens.Count - 2 &&
                                     tokens[tokens.Count - 1].IsEOS; 
                }
            }

            return result;
        }

        
        public virtual SyntaxParseResult<IN> ParseInfixExpressionRule(IList<Token<IN>> tokens, Rule<IN> rule, int position,
            string nonTerminalName)
        {
            var currentPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN>>();
            if (!tokens[position].IsEOS && rule.PossibleLeadingTokens.Contains(tokens[position].TokenID))
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    if (MatchExpressionRuleScheme(rule)) 
                    {
                        var first = rule.Clauses[0];
                        SyntaxParseResult<IN> firstResult = null;
                        if (first is NonTerminalClause<IN> firstNonTerminal)
                        {
                            firstResult = ParseNonTerminal(tokens, firstNonTerminal, currentPosition);

                            if (firstResult.IsError)
                            {
                                return firstResult;
                            }
                        }

                        currentPosition = firstResult.EndingPosition;
                        var second = rule.Clauses[1];
                        SyntaxParseResult<IN> secondResult = null;
                        if (second is ChoiceClause<IN> secondChoice)
                        {
                            secondResult = ParseChoice(tokens, secondChoice, currentPosition);

                            if (secondResult.IsError)
                            {
                                if (firstResult.Root is SyntaxNode<IN> node)
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
                        }

                        
                        
                        if (second is TerminalClause<IN> secondTerminal)
                        {
                            secondResult = ParseTerminal(tokens, secondTerminal, currentPosition);

                            if (secondResult.IsError)
                            {
                                if (firstResult.Root is SyntaxNode<IN> node)
                                {
                                    firstResult.Errors.AddRange(secondResult.Errors);
                                    firstResult.AddExpectings(secondResult.Expecting);
                                    return firstResult;
                                }
                            }
                        }
                        
                        
                        currentPosition = secondResult.EndingPosition;
                        var third = rule.Clauses[2];
                        SyntaxParseResult<IN> thirdResult;
                        if (third is NonTerminalClause<IN> thirdNonTerminal)
                        {
                            thirdResult = ParseNonTerminal(tokens, thirdNonTerminal, currentPosition);
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
                                var finalNode = new SyntaxNode<IN>( nonTerminalName,  children);
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
                    else
                    {
                        throw new ParserConfigurationException(
                            $@"expression rule {rule.RuleString} is incorrect : must have ""nonterminal terminal nonterminal"" scheme");
                    }
                    
                    
                }

            var result = new SyntaxParseResult<IN>();
            result.IsError = isError;
            result.Errors = errors;
            result.EndingPosition = currentPosition;
            if (!isError)
            {
                SyntaxNode<IN> node = null;
                if (rule.IsSubRule)
                    node = new GroupSyntaxNode<IN>(nonTerminalName, children);
                else
                    node = new SyntaxNode<IN>( nonTerminalName, children);
                node = ManageExpressionRules(rule, node);
                if (node.IsByPassNode) // inutile de créer un niveau supplémentaire
                    result.Root = children[0];
                result.Root = node;
                result.IsEnded = result.EndingPosition >= tokens.Count - 1
                                 || result.EndingPosition == tokens.Count - 2 &&
                                 tokens[tokens.Count - 1].IsEOS;
                
            }


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

        public SyntaxParseResult<IN> ParseZeroOrMore(IList<Token<IN>> tokens, ZeroOrMoreClause<IN> clause, int position)
        {
            var result = new SyntaxParseResult<IN>();
            var manyNode = new ManySyntaxNode<IN>("");
            var currentPosition = position;
            var innerClause = clause.Clause;
            var stillOk = true;
            

            SyntaxParseResult<IN> lastInnerResult = null;

            var innerErrors = new List<UnexpectedTokenSyntaxError<IN>>();

            bool hasByPasNodes = false;
            while (stillOk)
            {
                SyntaxParseResult<IN> innerResult = null;
                if (innerClause is TerminalClause<IN> term)
                {
                    manyNode.IsManyTokens = true;
                    innerResult = ParseTerminal(tokens, term, currentPosition);
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                }
                else if (innerClause is NonTerminalClause<IN> nonTerm)
                {
                    innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition);
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                    if (nonTerm.IsGroup)
                        manyNode.IsManyGroups = true;
                    else
                        manyNode.IsManyValues = true;
                }
                else if (innerClause is GroupClause<IN>)
                {
                    manyNode.IsManyGroups = true;
                    innerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<IN>, currentPosition);
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                }
                else if (innerClause is ChoiceClause<IN> choice)
                {
                    manyNode.IsManyTokens = choice.IsTerminalChoice;
                    manyNode.IsManyValues = choice.IsNonTerminalChoice;
                    innerResult = ParseChoice(tokens, choice, currentPosition);
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                }
                else
                {
                    throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
                }

                if (innerResult != null && !innerResult.IsError)
                {
                    manyNode.Add(innerResult.Root);
                    currentPosition = innerResult.EndingPosition;
                    lastInnerResult = innerResult;
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                }
                else
                {
                    if (innerResult != null)
                    {
                        innerErrors.AddRange(innerResult.Errors);
                    }
                }

                stillOk =  innerResult != null && !innerResult.IsError && currentPosition < tokens.Count;
            }


            result.EndingPosition = currentPosition;
            result.IsError = false;
            result.Errors = innerErrors;
            result.Root = manyNode;
            result.IsEnded = lastInnerResult != null && lastInnerResult.IsEnded;
            result.HasByPassNodes = hasByPasNodes;
            return result;
        }

        public SyntaxParseResult<IN> ParseOneOrMore(IList<Token<IN>> tokens, OneOrMoreClause<IN> clause, int position)
        {
            var result = new SyntaxParseResult<IN>();
            var manyNode = new ManySyntaxNode<IN>("");
            var currentPosition = position;
            var innerClause = clause.Clause;
            bool isError;

            SyntaxParseResult<IN> lastInnerResult = null;

            bool hasByPasNodes = false;
            SyntaxParseResult<IN> firstInnerResult = null;
            var innerErrors = new List<UnexpectedTokenSyntaxError<IN>>();

            if (innerClause is TerminalClause<IN>)
            {
                manyNode.IsManyTokens = true;
                firstInnerResult = ParseTerminal(tokens, innerClause as TerminalClause<IN>, currentPosition);
                hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
            }
            else if (innerClause is NonTerminalClause<IN>)
            {
                manyNode.IsManyValues = true;
                firstInnerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<IN>, currentPosition);
                hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
            }
            else if (innerClause is ChoiceClause<IN> choice)
            {
                manyNode.IsManyTokens = choice.IsTerminalChoice;
                manyNode.IsManyValues = choice.IsNonTerminalChoice;
                firstInnerResult = ParseChoice(tokens, choice, currentPosition);
                hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
            }
            else
            {
                throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
            }

            if (firstInnerResult != null && !firstInnerResult.IsError)
            {
                manyNode.Add(firstInnerResult.Root);
                lastInnerResult = firstInnerResult;
                currentPosition = firstInnerResult.EndingPosition;
                var more = new ZeroOrMoreClause<IN>(innerClause);
                var nextResult = ParseZeroOrMore(tokens, more, currentPosition);
                if (nextResult != null && !nextResult.IsError)
                {
                    currentPosition = nextResult.EndingPosition;
                    var moreChildren = (ManySyntaxNode<IN>) nextResult.Root;
                    manyNode.Children.AddRange(moreChildren.Children);
                }
                if (nextResult != null)
                {
                    innerErrors.AddRange(nextResult.Errors);
                }

                isError = false;
            }
            else
            {
                if (firstInnerResult != null)
                {
                    innerErrors.AddRange(firstInnerResult.Errors);
                }
                isError = true;
            }

            result.EndingPosition = currentPosition;
            result.IsError = isError;
            result.Errors = innerErrors;
            result.Root = manyNode;
            result.IsEnded = lastInnerResult != null && lastInnerResult.IsEnded;
            result.HasByPassNodes = hasByPasNodes;
            return result;
        }

        public SyntaxParseResult<IN> ParseOption(IList<Token<IN>> tokens, OptionClause<IN> clause, Rule<IN> rule,
            int position)
        {
            var result = new SyntaxParseResult<IN>();
            var currentPosition = position;
            var innerClause = clause.Clause;

            SyntaxParseResult<IN> innerResult = null;

            bool hasByPassNode = false;

            if (innerClause is TerminalClause<IN> term)
                innerResult = ParseTerminal(tokens, term, currentPosition);
            else if (innerClause is NonTerminalClause<IN> nonTerm)
                innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition);
            else if (innerClause is ChoiceClause<IN> choice)
            {
                innerResult = ParseChoice(tokens, choice, currentPosition);
            }
                
            else
                throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);


            if (innerResult.IsError)
            {
                if (innerClause is TerminalClause<IN>)
                {
                    result = new SyntaxParseResult<IN>();
                    result.IsError = true;
                    result.Root = new SyntaxLeaf<IN>(Token<IN>.Empty(),false);
                    result.EndingPosition = position;
                }
                else if (innerClause is ChoiceClause<IN> choiceClause)
                {
                    if (choiceClause.IsTerminalChoice)
                    {
                        result = new SyntaxParseResult<IN>();
                        result.IsError = false;
                        result.Root = new SyntaxLeaf<IN>(Token<IN>.Empty(),false);
                        result.EndingPosition = position;
                    }
                }
                else
                {
                    result = new SyntaxParseResult<IN>();
                    result.IsError = true;
                    var children = new List<ISyntaxNode<IN>> {innerResult.Root};
                    if (innerResult.IsError) children.Clear();
                    result.Root = new OptionSyntaxNode<IN>( rule.NonTerminalName, children,
                        rule.GetVisitor());
                    (result.Root as OptionSyntaxNode<IN>).IsGroupOption = clause.IsGroupOption;
                    result.EndingPosition = position;
                }
            }
            else
            {
                var node = innerResult.Root;
                
                var children = new List<ISyntaxNode<IN>> {innerResult.Root};
                result.Root =
                    new OptionSyntaxNode<IN>( rule.NonTerminalName,children, rule.GetVisitor());
                result.EndingPosition = innerResult.EndingPosition;
                result.HasByPassNodes = innerResult.HasByPassNodes;
            }

            return result;
        }
        
        public SyntaxParseResult<IN> ParseChoice(IList<Token<IN>> tokens, ChoiceClause<IN> choice, 
            int position)
        {
            var currentPosition = position;

            SyntaxParseResult<IN> result = new SyntaxParseResult<IN>()
            {
                IsError = true,
                IsEnded = false,
                EndingPosition = currentPosition
            };
             

            foreach (var alternate in choice.Choices)
            {
                if (alternate is TerminalClause<IN> terminalAlternate)
                {
                    result = ParseTerminal(tokens, terminalAlternate, currentPosition);
                }
                else if (alternate is NonTerminalClause<IN> nonTerminalAlternate)
                    result = ParseNonTerminal(tokens, nonTerminalAlternate, currentPosition);
                else
                    throw new InvalidOperationException("unable to apply repeater inside  " + choice.GetType().Name);
                if (result.IsOk)
                {
                    if (choice.IsTerminalChoice && choice.IsDiscarded && result.Root is SyntaxLeaf<IN> leaf)
                    {
                        var discardedToken = new SyntaxLeaf<IN>(leaf.Token, true);
                        result.Root = discardedToken;
                    }

                    return result;
                }               
            }

            if (result.IsError && choice.IsTerminalChoice)
            {
                var terminalAlternates = choice.Choices.Cast<TerminalClause<IN>>();
                var expected = terminalAlternates.Select(x => x.ExpectedToken).ToList();
                result.Errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition],I18n,expected.ToArray()));
            }
            
            return result;
        }

        #endregion
    }
}