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
            string nonTerminalName, SyntaxParsingContext<IN> parsingContext)
        {
            
            if (rule.IsInfixExpressionRule && rule.IsExpressionRule)
            {
                return ParseInfixExpressionRule(tokens, rule, position, nonTerminalName, parsingContext);
            }
            
            var currentPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN>>();
            if (rule.PossibleLeadingTokens.Any(x => x.Match(tokens[position])) || rule.MayBeEmpty)
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<IN>>();
                    foreach (var clause in rule.Clauses)
                    {
                        switch (clause)
                        {
                            case TerminalClause<IN> termClause:
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
                                    errors.Add(new UnexpectedTokenSyntaxError<IN>(tok,LexemeLabels, I18n,
                                        termClause.ExpectedToken));
                                }

                                isError = isError || termRes.IsError;
                                break;
                            }
                            case NonTerminalClause<IN> terminalClause:
                            {
                                var nonTerminalResult =
                                    ParseNonTerminal(tokens, terminalClause, currentPosition, parsingContext);
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
                                break;
                            }
                            case OneOrMoreClause<IN> _:
                            case ZeroOrMoreClause<IN> _:
                            {
                                SyntaxParseResult<IN> manyResult = null;
                                switch (clause)
                                {
                                    case OneOrMoreClause<IN> oneOrMore:
                                        manyResult = ParseOneOrMore(tokens, oneOrMore, currentPosition, parsingContext);
                                        break;
                                    case ZeroOrMoreClause<IN> zeroOrMore:
                                        manyResult = ParseZeroOrMore(tokens, zeroOrMore, currentPosition, parsingContext);
                                        break;
                                }
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
                                break;
                            }
                            case OptionClause<IN> option:
                            {
                                var optionResult = ParseOption(tokens, option, rule, currentPosition, parsingContext);
                                currentPosition = optionResult.EndingPosition;
                                children.Add(optionResult.Root);
                                break;
                            }
                            case ChoiceClause<IN> choice:
                            {
                                var choiceResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                                currentPosition = choiceResult.EndingPosition;
                                if (choiceResult.IsError && choiceResult.Errors != null && choiceResult.Errors.Any())
                                {
                                    errors.AddRange(choiceResult.Errors);
                                }

                                isError = choiceResult.IsError;

                                children.Add(choiceResult.Root);
                                break;
                            }
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
                    result.IsEnded = tokens[result.EndingPosition].IsEOS;
                }
            }

            return result;
        }


        public virtual SyntaxParseResult<IN> ParseInfixExpressionRule(IList<Token<IN>> tokens, Rule<IN> rule,
            int position,
            string nonTerminalName, SyntaxParsingContext<IN> parsingContext)
        {
            var currentPosition = position;
            var errors = new List<UnexpectedTokenSyntaxError<IN>>();
            var isError = false;
            var children = new List<ISyntaxNode<IN>>();
            if (!tokens[position].IsEOS && rule.PossibleLeadingTokens.Any(x => x.Match(tokens[position])))
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    if (MatchExpressionRuleScheme(rule))
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
                            case TerminalClause<IN> secondTerminal:
                            {
                                secondResult = ParseTerminal(tokens, secondTerminal, currentPosition, parsingContext);

                                if (secondResult.IsError)
                                {
                                    if (firstResult.Root is SyntaxNode<IN>)
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

        public SyntaxParseResult<IN> ParseZeroOrMore(IList<Token<IN>> tokens, ZeroOrMoreClause<IN> clause, int position,
            SyntaxParsingContext<IN> parsingContext)
        {
            if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
            {
                return parseResult;
            }
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
                switch (innerClause)
                {
                    case TerminalClause<IN> term:
                        manyNode.IsManyTokens = true;
                        innerResult = ParseTerminal(tokens, term, currentPosition,parsingContext);
                        hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                        break;
                    case NonTerminalClause<IN> nonTerm:
                    {
                        innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                        hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                        if (nonTerm.IsGroup)
                            manyNode.IsManyGroups = true;
                        else
                            manyNode.IsManyValues = true;
                        break;
                    }
                    case GroupClause<IN> _:
                        manyNode.IsManyGroups = true;
                        innerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<IN>, currentPosition, parsingContext);
                        hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                        break;
                    case ChoiceClause<IN> choice:
                        manyNode.IsManyTokens = choice.IsTerminalChoice;
                        manyNode.IsManyValues = choice.IsNonTerminalChoice;
                        innerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                        hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                        break;
                    default:
                        throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
                }

                if (innerResult != null && !innerResult.IsError)
                {
                    manyNode.Add(innerResult.Root);
                    currentPosition = innerResult.EndingPosition;
                    lastInnerResult = innerResult;
                    hasByPasNodes = hasByPasNodes || innerResult.HasByPassNodes;
                    innerErrors.AddRange(lastInnerResult.Errors);
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
            parsingContext.Memoize(clause,position,result);
            return result;
        }

        public SyntaxParseResult<IN> ParseOneOrMore(IList<Token<IN>> tokens, OneOrMoreClause<IN> clause, int position,
            SyntaxParsingContext<IN> parsingContext)
        {
            if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
            {
                return parseResult;
            }
            
            var result = new SyntaxParseResult<IN>();
            var manyNode = new ManySyntaxNode<IN>("");
            var currentPosition = position;
            var innerClause = clause.Clause;
            bool isError;

            SyntaxParseResult<IN> lastInnerResult = null;

            bool hasByPasNodes = false;
            SyntaxParseResult<IN> firstInnerResult = null;
            var innerErrors = new List<UnexpectedTokenSyntaxError<IN>>();

            switch (innerClause)
            {
                case TerminalClause<IN> terminalClause:
                    manyNode.IsManyTokens = true;
                    firstInnerResult = ParseTerminal(tokens, terminalClause, currentPosition, parsingContext);
                    hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
                    break;
                case NonTerminalClause<IN> nonTerm:
                {
                    firstInnerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                    hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
                    if (nonTerm.IsGroup)
                        manyNode.IsManyGroups = true;
                    else
                        manyNode.IsManyValues = true;
                    break;
                }
                case ChoiceClause<IN> choice:
                    manyNode.IsManyTokens = choice.IsTerminalChoice;
                    manyNode.IsManyValues = choice.IsNonTerminalChoice;
                    firstInnerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                    hasByPasNodes = hasByPasNodes || firstInnerResult.HasByPassNodes;
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
            }

            if (firstInnerResult != null && !firstInnerResult.IsError)
            {
                manyNode.Add(firstInnerResult.Root);
                lastInnerResult = firstInnerResult;
                currentPosition = firstInnerResult.EndingPosition;
                var more = new ZeroOrMoreClause<IN>(innerClause);
                var nextResult = ParseZeroOrMore(tokens, more, currentPosition, parsingContext);
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
            parsingContext.Memoize(clause,position,result);
            return result;
        }

        public SyntaxParseResult<IN> ParseOption(IList<Token<IN>> tokens, OptionClause<IN> clause, Rule<IN> rule,
            int position, SyntaxParsingContext<IN> parsingContext)
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
                case TerminalClause<IN> term:
                    innerResult = ParseTerminal(tokens, term, currentPosition, parsingContext);
                    break;
                case NonTerminalClause<IN> nonTerm:
                    innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition, parsingContext);
                    break;
                case ChoiceClause<IN> choice:
                    innerResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                    break;
                default:
                    throw new InvalidOperationException("unable to apply repeater to " + innerClause.GetType().Name);
            }


            if (innerResult.IsError)
            {
                switch (innerClause)
                {
                    case TerminalClause<IN> _:
                        result = new SyntaxParseResult<IN>();
                        result.IsError = true;
                        result.Root = new SyntaxLeaf<IN>(Token<IN>.Empty(),false);
                        result.EndingPosition = position;
                        break;
                    case ChoiceClause<IN> choiceClause:
                    {
                        if (choiceClause.IsTerminalChoice)
                        {
                            result = new SyntaxParseResult<IN>();
                            result.IsError = false;
                            result.Root = new SyntaxLeaf<IN>(Token<IN>.Empty(),false);
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
                        var children = new List<ISyntaxNode<IN>> {innerResult.Root};
                        if (innerResult.IsError) children.Clear();
                        result.Root = new OptionSyntaxNode<IN>( rule.NonTerminalName, children,
                            rule.GetVisitor());
                        (result.Root as OptionSyntaxNode<IN>).IsGroupOption = clause.IsGroupOption;
                        result.EndingPosition = position;
                        break;
                    }
                }
            }
            else
            {
                var children = new List<ISyntaxNode<IN>> {innerResult.Root};
                result.Root =
                    new OptionSyntaxNode<IN>( rule.NonTerminalName,children, rule.GetVisitor());
                result.EndingPosition = innerResult.EndingPosition;
                result.HasByPassNodes = innerResult.HasByPassNodes;
            }

            parsingContext.Memoize(clause, position, result);
            return result;
        }
        
        public SyntaxParseResult<IN> ParseChoice(IList<Token<IN>> tokens, ChoiceClause<IN> clause,
            int position, SyntaxParsingContext<IN> parsingContext)
        {
            if (parsingContext.TryGetParseResult(clause, position, out var parseResult))
            {
                return parseResult;
            }
            var currentPosition = position;

            SyntaxParseResult<IN> result = new SyntaxParseResult<IN>
            {
                IsError = true,
                IsEnded = false,
                EndingPosition = currentPosition
            };
             

            foreach (var alternate in clause.Choices)
            {
                switch (alternate)
                {
                    case TerminalClause<IN> terminalAlternate:
                        result = ParseTerminal(tokens, terminalAlternate, currentPosition, parsingContext);
                        break;
                    case NonTerminalClause<IN> nonTerminalAlternate:
                        result = ParseNonTerminal(tokens, nonTerminalAlternate, currentPosition, parsingContext);
                        break;
                    default:
                        throw new InvalidOperationException("unable to apply repeater inside  " + clause.GetType().Name);
                }

                if (result.IsOk)
                {
                    if (clause.IsTerminalChoice && clause.IsDiscarded && result.Root is SyntaxLeaf<IN> leaf)
                    {
                        var discardedToken = new SyntaxLeaf<IN>(leaf.Token, true);
                        result.Root = discardedToken;
                    }
                    parsingContext.Memoize(clause,position,result);
                    return result;
                }
            }

            if (result.IsError && clause.IsTerminalChoice)
            {
                var terminalAlternates = clause.Choices.Cast<TerminalClause<IN>>();
                var expected = terminalAlternates.Select(x => x.ExpectedToken).ToList();
                result.Errors.Add(new UnexpectedTokenSyntaxError<IN>(tokens[currentPosition], LexemeLabels, I18n,expected.ToArray()));
            }
            parsingContext.Memoize(clause,position,result);
            return result;
        }

        #endregion
    }
}