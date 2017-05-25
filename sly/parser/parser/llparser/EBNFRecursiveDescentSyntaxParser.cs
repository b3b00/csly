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
    public class EBNFRecursiveDescentSyntaxParser<T> : RecursiveDescentSyntaxParser<T>
    {
        //public ParserConfiguration<T> Configuration { get; set; }

        //public string StartingNonTerminal { get; set; }


        public EBNFRecursiveDescentSyntaxParser(ParserConfiguration<T> configuration, string startingNonTerminal) : base(configuration,startingNonTerminal)
        {
            Configuration = configuration;
            StartingNonTerminal = startingNonTerminal;
            InitializeStartingTokens(Configuration, startingNonTerminal);
        }

        #region STARTING_TOKENS

        

        protected override  void InitStartingTokensForRule(Dictionary<string, NonTerminal<T>> nonTerminals, Rule<T> rule)
        {
            if (rule.PossibleLeadingTokens == null || rule.PossibleLeadingTokens.Count == 0)
            {
                rule.PossibleLeadingTokens = new List<T>();
                if (rule.Clauses.Count > 0)
                {
                    IClause<T> first = rule.Clauses[0];
                    if (first is TerminalClause<T>)
                    {
                        TerminalClause<T> term = first as TerminalClause<T>;

                        InitStartingTokensWithTerminal(rule, term);
                    }
                    else if (first is NonTerminalClause<T>)
                    {
                        NonTerminalClause<T> nonterm = first as NonTerminalClause<T>;
                        InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
                    }
                    else if (first is ZeroOrMoreClause<T>)
                    {
                        ZeroOrMoreClause<T> many = first as ZeroOrMoreClause<T>;
                        InitStartingTokensWithZeroOrMore(rule, many, nonTerminals);
                    }
                    else if (first is OneOrMoreClause<T>)
                    {
                        OneOrMoreClause<T> many = first as OneOrMoreClause<T>;
                        InitStartingTokensWithOneOrMore(rule, many, nonTerminals);
                    }
                }
            }
        }


        private void InitStartingTokensWithTerminal(Rule<T> rule, TerminalClause<T> term)
        {
            rule.PossibleLeadingTokens.Add(term.ExpectedToken);
            rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct<T>().ToList<T>();
        }

        private void InitStartingTokensWithNonTerminal(Rule<T> rule, NonTerminalClause<T> nonterm,
            Dictionary<string, NonTerminal<T>> nonTerminals)
        {
            InitStartingTokensForNonTerminal(nonTerminals, nonterm.NonTerminalName);
            if (nonTerminals.ContainsKey(nonterm.NonTerminalName))
            {
                NonTerminal<T> firstNonTerminal = nonTerminals[nonterm.NonTerminalName];
                firstNonTerminal.Rules.ForEach(r => { rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens); });
                rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct<T>().ToList<T>();
            }
        }

        private void InitStartingTokensWithZeroOrMore(Rule<T> rule, ZeroOrMoreClause<T> manyClause,
            Dictionary<string, NonTerminal<T>> nonTerminals)
        {
            if (manyClause.Clause is TerminalClause<T>)
            {
                TerminalClause<T> term = manyClause.Clause as TerminalClause<T>;

                InitStartingTokensWithTerminal(rule, term);
            }
            else if (manyClause.Clause is NonTerminalClause<T>)
            {
                NonTerminalClause<T> nonterm = manyClause.Clause as NonTerminalClause<T>;
                InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
            }
        }

        private void InitStartingTokensWithOneOrMore(Rule<T> rule, OneOrMoreClause<T> manyClause,
            Dictionary<string, NonTerminal<T>> nonTerminals)
        {
            if (manyClause.Clause is TerminalClause<T>)
            {
                TerminalClause<T> term = manyClause.Clause as TerminalClause<T>;

                InitStartingTokensWithTerminal(rule, term);
            }
            else if (manyClause.Clause is NonTerminalClause<T>)
            {
                NonTerminalClause<T> nonterm = manyClause.Clause as NonTerminalClause<T>;
                InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
            }
        }

        #endregion

        #region parsing
        

        public override SyntaxParseResult<T> Parse(IList<Token<T>> tokens, Rule<T> rule, int position, string nonTerminalName)
        {
            int currentPosition = position;
            List<UnexpectedTokenSyntaxError<T>> errors = new List<UnexpectedTokenSyntaxError<T>>();
            bool isError = false;
            List<ISyntaxNode<T>> children = new List<ISyntaxNode<T>>();
            if (rule.PossibleLeadingTokens.Contains(tokens[position].TokenID) || rule.MayBeEmpty)
            {
                if (rule.Clauses != null && rule.Clauses.Count > 0)
                {
                    children = new List<ISyntaxNode<T>>();
                    foreach (IClause<T> clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<T>)
                        {
                            SyntaxParseResult<T> termRes =
                                ParseTerminal(tokens, clause as TerminalClause<T>, currentPosition);
                            if (!termRes.IsError)
                            {
                                children.Add(termRes.Root);
                                currentPosition = termRes.EndingPosition;
                            }
                            else
                            {
                                Token<T> tok = tokens[currentPosition];
                                errors.Add(new UnexpectedTokenSyntaxError<T>(tok,
                                    ((TerminalClause<T>) clause).ExpectedToken));
                            }
                            isError = isError || termRes.IsError;
                        }
                        else if (clause is NonTerminalClause<T>)
                        {
                            SyntaxParseResult<T> nonTerminalResult =
                                ParseNonTerminal(tokens, clause as NonTerminalClause<T>, currentPosition);
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
                            // TODO
                        }

                        else if (clause is ZeroOrMoreClause<T>)
                        {
                            SyntaxParseResult<T> zeroOrMoreResult =
                                ParseZeroOrMore(tokens, clause as ZeroOrMoreClause<T>, currentPosition);
                            if (!zeroOrMoreResult.IsError)
                            {
                                children.Add(zeroOrMoreResult.Root);
                                currentPosition = zeroOrMoreResult.EndingPosition;
                            }
                            else
                            {
                                errors.AddRange(zeroOrMoreResult.Errors);
                            }
                            isError = isError || zeroOrMoreResult.IsError;
                        }
                        else if (clause is OneOrMoreClause<T>)
                        {
                            SyntaxParseResult<T> oneMoreResult =
                                ParseOneOrMore(tokens, clause as OneOrMoreClause<T>, currentPosition);
                            if (!oneMoreResult.IsError)
                            {
                                children.Add(oneMoreResult.Root);
                                currentPosition = oneMoreResult.EndingPosition;
                            }
                            else
                            {
                                errors.AddRange(oneMoreResult.Errors);
                            }
                            isError = isError || oneMoreResult.IsError;
                        }
                        if (isError)
                        {
                            break;
                        }
                    }
                }
            }

            SyntaxParseResult<T> result = new SyntaxParseResult<T>();
            result.IsError = isError;
            result.Errors = errors;
            result.EndingPosition = currentPosition;
            if (!isError)
            {
                SyntaxNode<T> node = new SyntaxNode<T>(nonTerminalName + "__" + rule.Key, children);
                result.Root = node;
                result.IsEnded = currentPosition >= tokens.Count - 1
                                 || currentPosition == tokens.Count - 2 &&
                                 tokens[tokens.Count - 1].TokenID.Equals(default(T));
            }

            return result;
        }

      
        public SyntaxParseResult<T> ParseZeroOrMore(IList<Token<T>> tokens, ZeroOrMoreClause<T> clause, int position)
        {
            SyntaxParseResult<T> result = new SyntaxParseResult<T>();
            ManySyntaxNode<T> manyNode = new ManySyntaxNode<T>("");
            int currentPosition = position;
            IClause<T> innerClause = clause.Clause;
            bool stillOk = true;

            SyntaxParseResult<T> lastInnerResult = null;

            while (stillOk)
            {
                SyntaxParseResult<T> innerResult = null;
                if (innerClause is TerminalClause<T>)
                {
                    innerResult = ParseTerminal(tokens, innerClause as TerminalClause<T>, currentPosition);
                }
                else if (innerClause is NonTerminalClause<T>)
                {
                    innerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<T>, currentPosition);
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

        public SyntaxParseResult<T> ParseOneOrMore(IList<Token<T>> tokens, OneOrMoreClause<T> clause, int position)
        {
            SyntaxParseResult<T> result = new SyntaxParseResult<T>();
            ManySyntaxNode<T> manyNode = new ManySyntaxNode<T>("");
            int currentPosition = position;
            IClause<T> innerClause = clause.Clause;
            bool isError;

            SyntaxParseResult<T> lastInnerResult = null;

            SyntaxParseResult<T> firstInnerResult = null;
            if (innerClause is TerminalClause<T>)
            {
                firstInnerResult = ParseTerminal(tokens, innerClause as TerminalClause<T>, currentPosition);
            }
            else if (innerClause is NonTerminalClause<T>)
            {
                firstInnerResult = ParseNonTerminal(tokens, innerClause as NonTerminalClause<T>, currentPosition);
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
                ZeroOrMoreClause<T> more = new ZeroOrMoreClause<T>(innerClause);
                SyntaxParseResult<T> nextResult = ParseZeroOrMore(tokens, more, currentPosition);
                if (nextResult != null && !nextResult.IsError)
                {
                    currentPosition = nextResult.EndingPosition;
                    ManySyntaxNode<T> moreChildren = (ManySyntaxNode<T>) nextResult.Root;
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
