using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.tree;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser
{
    public class EBNFRecursiveDescentSyntaxParser<IN, OUT> : RecursiveDescentSyntaxParser<IN, OUT> where IN : struct
    {
        public EBNFRecursiveDescentSyntaxParser(ParserConfiguration<IN, OUT> configuration, string startingNonTerminal)
            : base(configuration, startingNonTerminal)
        {
        }

        #region STARTING_TOKENS

        protected override void InitStartingTokensForRuleExtensions(IClause<IN> first, Rule<IN> rule,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            
            if (first is TerminalClause<IN>)
            {
                var term = first as TerminalClause<IN>;

                InitStartingTokensWithTerminal(rule, term);
            }
            else if (first is NonTerminalClause<IN>)
            {
                var nonterm = first as NonTerminalClause<IN>;
                InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
            }
            else if (first is ZeroOrMoreClause<IN> zeroOrMore)
            {
                InitStartingTokensWithZeroOrMore(rule, zeroOrMore, nonTerminals);
                int i = 1;
                bool optional = true;
                while (i < rule.Clauses.Count && optional)
                {
                    IClause<IN> clause = rule.Clauses[i];

                    switch (clause)
                    {
                        case TerminalClause<IN> terminalClause:
                        {
                            rule.PossibleLeadingTokens.Add(terminalClause.ExpectedToken);
                            break;
                        }
                        case NonTerminalClause<IN> terminalClause:
                        {
                            InitStartingTokensForNonTerminal(nonTerminals, terminalClause.NonTerminalName);
                            NonTerminal<IN> nonTerminal = nonTerminals[terminalClause.NonTerminalName];
                            {
                                rule.PossibleLeadingTokens.AddRange(nonTerminal.PossibleLeadingTokens);
                            }
                            break;
                        }
                        case ChoiceClause<IN> choice:
                        {
                            InitStartingTokensWithChoice(rule, choice, nonTerminals);
                            break;
                        }
                    }

                    // add startig tokens of clause in rule.startingtokens
                    optional = clause is ZeroOrMoreClause<IN> || clause is OptionClause<IN>;
                    i++;
                }
            }
            else if (first is OneOrMoreClause<IN>)
            {
                var many = first as OneOrMoreClause<IN>;
                InitStartingTokensWithOneOrMore(rule, many, nonTerminals);
            }
            else if (first is ChoiceClause<IN> choice)
            {
                InitStartingTokensWithChoice(rule, choice, nonTerminals);
            }
        }

        private void InitStartingTokensWithChoice(Rule<IN> rule, ChoiceClause<IN> choice,Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            foreach (var alternate in choice.Choices)
            {
                if (alternate is TerminalClause<IN> term)
                {
                    InitStartingTokensWithTerminal(rule,term);
                }
                else if (alternate is NonTerminalClause<IN> nonTerminal)
                {
                    InitStartingTokensWithNonTerminal(rule,nonTerminal,nonTerminals);
                }
            }
        }


        private void InitStartingTokensWithTerminal(Rule<IN> rule, TerminalClause<IN> term)
        {
            rule.PossibleLeadingTokens.Add(term.ExpectedToken);
            rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
        }

        private void InitStartingTokensWithNonTerminal(Rule<IN> rule, NonTerminalClause<IN> nonterm,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            InitStartingTokensForNonTerminal(nonTerminals, nonterm.NonTerminalName);
            if (nonTerminals.ContainsKey(nonterm.NonTerminalName))
            {
                var firstNonTerminal = nonTerminals[nonterm.NonTerminalName];
                firstNonTerminal.Rules.ForEach(r => { rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens); });
                rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
            }
        }

        private void InitStartingTokensWithZeroOrMore(Rule<IN> rule, ZeroOrMoreClause<IN> manyClause,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            if (manyClause.Clause is TerminalClause<IN> term)
            {
                InitStartingTokensWithTerminal(rule, term);
            }
            else if (manyClause.Clause is NonTerminalClause<IN> nonTerm)
            {
                InitStartingTokensWithNonTerminal(rule, nonTerm, nonTerminals);
            }
            else if (manyClause.Clause is ChoiceClause<IN> choice)
            {
                InitStartingTokensWithChoice(rule,choice,nonTerminals);
            }
        }

        private void InitStartingTokensWithOneOrMore(Rule<IN> rule, OneOrMoreClause<IN> manyClause,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            if (manyClause.Clause is TerminalClause<IN> term)
            {
                InitStartingTokensWithTerminal(rule, term);
            }
            else if (manyClause.Clause is NonTerminalClause<IN> nonterm)
            {
                InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
            }
            else if (manyClause.Clause is ChoiceClause<IN> choice)
            {
                InitStartingTokensWithChoice(rule, choice, nonTerminals);
            }
        }

        #endregion

        #region parsing

        public override SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, Rule<IN> rule, int position,
            string nonTerminalName)
        {
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
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok,
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
                            var optionResult = ParseChoice(tokens, choice, currentPosition);
                            currentPosition = optionResult.EndingPosition;
                            if (optionResult.IsError && optionResult.Errors != null && optionResult.Errors.Count > 0)
                            {
                                errors.AddRange(optionResult.Errors);
                            }

                            isError = optionResult.IsError;

                            children.Add(optionResult.Root);
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
                                     tokens[tokens.Count - 1].TokenID.Equals(default(IN));
                }
                else
                {
                    node = new SyntaxNode<IN>( nonTerminalName,  children);
                    node.ExpressionAffix = rule.ExpressionAffix;
                    node = ManageExpressionRules(rule, node);
                    result.Root = node;
                    result.IsEnded = currentPosition >= tokens.Count - 1
                                     || currentPosition == tokens.Count - 2 &&
                                     tokens[tokens.Count - 1].TokenID.Equals(default(IN));
                }
            }

            return result;
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

            while (stillOk)
            {
                SyntaxParseResult<IN> innerResult = null;
                if (innerClause is TerminalClause<IN> term)
                {
                    manyNode.IsManyTokens = true;
                    innerResult = ParseTerminal(tokens, term, currentPosition);
                }
                else if (innerClause is NonTerminalClause<IN> nonTerm)
                {
                    innerResult = ParseNonTerminal(tokens, nonTerm, currentPosition);
                    if (nonTerm.IsGroup)
                        manyNode.IsManyGroups = true;
                    else
                        manyNode.IsManyValues = true;
                }
                else if (innerClause is GroupClause<IN>)
                {
                    manyNode.IsManyGroups = true;
                    innerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<IN>, currentPosition);
                }
                else if (innerClause is ChoiceClause<IN> choice)
                {
                    manyNode.IsManyTokens = choice.IsTerminalChoice;
                    manyNode.IsManyValues = choice.IsNonTerminalChoice;
                    innerResult = ParseChoice(tokens, choice, currentPosition);
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

            SyntaxParseResult<IN> firstInnerResult = null;
            if (innerClause is TerminalClause<IN>)
            {
                manyNode.IsManyTokens = true;
                firstInnerResult = ParseTerminal(tokens, innerClause as TerminalClause<IN>, currentPosition);
            }
            else if (innerClause is NonTerminalClause<IN>)
            {
                manyNode.IsManyValues = true;
                firstInnerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<IN>, currentPosition);
            }
            else if (innerClause is ChoiceClause<IN> choice)
            {
                manyNode.IsManyTokens = choice.IsTerminalChoice;
                manyNode.IsManyValues = choice.IsNonTerminalChoice;
                firstInnerResult = ParseChoice(tokens, choice, currentPosition);
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

                isError = false;
            }

            else
            {
                isError = true;
            }

            result.EndingPosition = currentPosition;
            result.IsError = isError;
            result.Root = manyNode;
            result.IsEnded = lastInnerResult != null && lastInnerResult.IsEnded;
            return result;
        }

        public SyntaxParseResult<IN> ParseOption(IList<Token<IN>> tokens, OptionClause<IN> clause, Rule<IN> rule,
            int position)
        {
            var result = new SyntaxParseResult<IN>();
            var currentPosition = position;
            var innerClause = clause.Clause;

            SyntaxParseResult<IN> innerResult = null;


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

            return result;
        }

        #endregion
    }
}