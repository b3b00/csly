using System;
using sly.parser.syntax;
using sly.lexer;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using sly.parser.generator;

namespace sly.parser.llparser
{
    public class EBNFRecursiveDescentSyntaxParser<IN,OUT> : RecursiveDescentSyntaxParser<IN,OUT> where IN : struct
    {
    
        public EBNFRecursiveDescentSyntaxParser(ParserConfiguration<IN,OUT> configuration, string startingNonTerminal) : base(configuration,startingNonTerminal)
        {
            Configuration = configuration;
            StartingNonTerminal = startingNonTerminal;
            InitializeStartingTokens(Configuration, startingNonTerminal);
        }

        #region STARTING_TOKENS

        

        protected override  void InitStartingTokensForRule(Dictionary<string, NonTerminal<IN>> nonTerminals, Rule<IN> rule)
        {
            if (rule.PossibleLeadingTokens == null || rule.PossibleLeadingTokens.Count == 0)
            {
                rule.PossibleLeadingTokens = new List<IN>();
                if (rule.Clauses.Count > 0)
                {
                    IClause<IN> first = rule.Clauses[0];
                    if (first is TerminalClause<IN>)
                    {
                        TerminalClause<IN> term = first as TerminalClause<IN>;

                        InitStartingTokensWithTerminal(rule, term);
                    }
                    else if (first is NonTerminalClause<IN>)
                    {
                        NonTerminalClause<IN> nonterm = first as NonTerminalClause<IN>;
                        InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
                    }
                    else if (first is ZeroOrMoreClause<IN>)
                    {
                        ZeroOrMoreClause<IN> many = first as ZeroOrMoreClause<IN>;
                        InitStartingTokensWithZeroOrMore(rule, many, nonTerminals);
                    }
                    else if (first is OneOrMoreClause<IN>)
                    {
                        OneOrMoreClause<IN> many = first as OneOrMoreClause<IN>;
                        InitStartingTokensWithOneOrMore(rule, many, nonTerminals);
                    }
                }
            }
        }


        private void InitStartingTokensWithTerminal(Rule<IN> rule, TerminalClause<IN> term)
        {
            rule.PossibleLeadingTokens.Add(term.ExpectedToken);
            rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct<IN>().ToList<IN>();
        }

        private void InitStartingTokensWithNonTerminal(Rule<IN> rule, NonTerminalClause<IN> nonterm,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            InitStartingTokensForNonTerminal(nonTerminals, nonterm.NonTerminalName);
            if (nonTerminals.ContainsKey(nonterm.NonTerminalName))
            {
                NonTerminal<IN> firstNonTerminal = nonTerminals[nonterm.NonTerminalName];
                firstNonTerminal.Rules.ForEach(r => { rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens); });
                rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct<IN>().ToList<IN>();
            }
        }

        private void InitStartingTokensWithZeroOrMore(Rule<IN> rule, ZeroOrMoreClause<IN> manyClause,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            if (manyClause.Clause is TerminalClause<IN>)
            {
                TerminalClause<IN> term = manyClause.Clause as TerminalClause<IN>;

                InitStartingTokensWithTerminal(rule, term);
            }
            else if (manyClause.Clause is NonTerminalClause<IN>)
            {
                NonTerminalClause<IN> nonterm = manyClause.Clause as NonTerminalClause<IN>;
                InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
            }
        }

        private void InitStartingTokensWithOneOrMore(Rule<IN> rule, OneOrMoreClause<IN> manyClause,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            if (manyClause.Clause is TerminalClause<IN>)
            {
                TerminalClause<IN> term = manyClause.Clause as TerminalClause<IN>;

                InitStartingTokensWithTerminal(rule, term);
            }
            else if (manyClause.Clause is NonTerminalClause<IN>)
            {
                NonTerminalClause<IN> nonterm = manyClause.Clause as NonTerminalClause<IN>;
                InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
            }
        }

        #endregion

        #region parsing
        

        public override SyntaxParseResult<IN> Parse(IList<Token<IN>> tokens, Rule<IN> rule, int position, string nonTerminalName)
        {
            int currentPosition = position;
            List<UnexpectedTokenSyntaxError<IN>> errors = new List<UnexpectedTokenSyntaxError<IN>>();
            bool isError = false;
            List<ISyntaxNode<IN>> children = new List<ISyntaxNode<IN>>();
            if (rule.PossibleLeadingTokens.Contains(tokens[position].TokenID) || rule.MayBeEmpty)
            {
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<IN>>();
                    foreach (IClause<IN> clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<IN>)
                        {
                            SyntaxParseResult<IN> termRes =
                                ParseTerminal(tokens, clause as TerminalClause<IN>, currentPosition);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            else
                            {
                                Token<IN> tok = tokens[currentPosition];
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok,
                                    ((TerminalClause<IN>) clause).ExpectedToken));
                            }
                            isError = isError || termRes.IsError;
                        }
                        else if (clause is NonTerminalClause<IN>)
                        {
                            SyntaxParseResult<IN> nonTerminalResult =
                                ParseNonTerminal(tokens, clause as NonTerminalClause<IN>, currentPosition);
                            if (!nonTerminalResult.IsError)
                            {
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
                            if (clause is OneOrMoreClause<IN> oneOrMore) {
                                manyResult = ParseOneOrMore(tokens, oneOrMore, currentPosition);
                            }
                            else if (clause is ZeroOrMoreClause<IN> zeroOrMore) {
                                manyResult = ParseZeroOrMore(tokens, zeroOrMore, currentPosition);
                            }
                            if (!manyResult.IsError)
                            {
                                children.Add(manyResult.Root);
                                currentPosition = manyResult.EndingPosition;
                            }
                            else
                            {
                                if (manyResult.Errors != null && manyResult.Errors.Count > 0)
                                {
                                    errors.AddRange(manyResult.Errors);
                                }                                
                            }
                            isError = isError || manyResult.IsError;
                        }
                        if (isError)
                        {
                            break;
                        }
                    }
                }
            }

            SyntaxParseResult<IN> result = new SyntaxParseResult<IN>();
            result.IsError = isError;
            result.Errors = errors;
            result.EndingPosition = currentPosition;
            if (!isError)
            {
                SyntaxNode<IN> node = new SyntaxNode<IN>(nonTerminalName + "__" + rule.Key, children);
                node = ManageExpressionRules(rule, node);
                result.Root = node;
                result.IsEnded = currentPosition >= tokens.Count - 1
                                 || currentPosition == tokens.Count - 2 &&
                                 tokens[tokens.Count - 1].TokenID.Equals(default(IN));
            }

            return result;
        }

      
        public SyntaxParseResult<IN> ParseZeroOrMore(IList<Token<IN>> tokens, ZeroOrMoreClause<IN> clause, int position)
        {
            SyntaxParseResult<IN> result = new SyntaxParseResult<IN>();
            ManySyntaxNode<IN> manyNode = new ManySyntaxNode<IN>("");
            int currentPosition = position;
            IClause<IN> innerClause = clause.Clause;
            bool stillOk = true;

            SyntaxParseResult<IN> lastInnerResult = null;

            while (stillOk)
            {
                SyntaxParseResult<IN> innerResult = null;
                if (innerClause is TerminalClause<IN>)
                {
                    manyNode.IsManyTokens = true;
                    innerResult = ParseTerminal(tokens, innerClause as TerminalClause<IN>, currentPosition);
                }
                else if (innerClause is NonTerminalClause<IN>)
                {
                    manyNode.IsManyValues = true;
                    innerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<IN>, currentPosition);
                }
                else
                {
                    throw new NotImplementedException("unable to apply repeater to " + innerClause.GetType().Name);
                }
                if (innerResult != null && !innerResult.IsError)
                {
                    manyNode.Add(innerResult.Root);
                    currentPosition = innerResult.EndingPosition;
                    lastInnerResult = innerResult;
                }
                stillOk = stillOk && innerResult != null && !(innerResult.IsError);
            }


            result.EndingPosition = currentPosition;
            result.IsError = false;
            result.Root = manyNode;
            result.IsEnded = lastInnerResult != null && lastInnerResult.IsEnded;
            return result;
        }

        public SyntaxParseResult<IN> ParseOneOrMore(IList<Token<IN>> tokens, OneOrMoreClause<IN> clause, int position)
        {
            SyntaxParseResult<IN> result = new SyntaxParseResult<IN>();
            ManySyntaxNode<IN> manyNode = new ManySyntaxNode<IN>("");
            int currentPosition = position;
            IClause<IN> innerClause = clause.Clause;
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
            else
            {
                throw new NotImplementedException("unable to apply repeater to " + innerClause.GetType().Name);
            }
            if (firstInnerResult != null && !firstInnerResult.IsError)
            {
                manyNode.Add(firstInnerResult.Root);
                lastInnerResult = firstInnerResult;
                currentPosition = firstInnerResult.EndingPosition;
                ZeroOrMoreClause<IN> more = new ZeroOrMoreClause<IN>(innerClause);
                SyntaxParseResult<IN> nextResult = ParseZeroOrMore(tokens, more, currentPosition);
                if (nextResult != null && !nextResult.IsError)
                {
                    currentPosition = nextResult.EndingPosition;
                    ManySyntaxNode<IN> moreChildren = (ManySyntaxNode<IN>) nextResult.Root;
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

     

        #endregion
    }
}
