﻿using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.tree;
using sly.parser.syntax.grammar;
using sly.parser.llparser.bnf;

namespace sly.parser.llparser.ebnf
{
    public partial  class EBNFRecursiveDescentSyntaxParser<IN, OUT> : RecursiveDescentSyntaxParser<IN, OUT> where IN : struct, Enum
    {
        public EBNFRecursiveDescentSyntaxParser(ParserConfiguration<IN, OUT> configuration, string startingNonTerminal, string i18n)
            : base(configuration, startingNonTerminal, i18n)
        {
        }


        #region parsing

        public override SyntaxParseResult<IN, OUT> Parse(Token<IN>[] tokens, Rule<IN, OUT> rule, int position,
            string nonTerminalName, SyntaxParsingContext<IN, OUT> parsingContext)
        {
            if (rule.IsInfixExpressionRule && rule.IsExpressionRule)
            {
                return ParseInfixExpressionRule(tokens, rule, position, nonTerminalName, parsingContext);
            }
            
            var currentPosition = position;
            // Optimization: Pre-allocate error list with reasonable capacity
            var errors = new List<UnexpectedTokenSyntaxError<IN>>(4);
            var isError = false;
            
            // Optimization: Pre-allocate children list based on clauses count
            var children = new List<ISyntaxNode<IN, OUT>>(rule.Clauses?.Count ?? 0);
            
            if (rule.Match(tokens, position, Configuration) && rule.Clauses != null && rule.Clauses.Count > 0)
            {
                foreach (var clause in rule.Clauses)
                {
                    switch (clause)
                    {
                        case TerminalClause<IN, OUT> termClause:
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
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok, LexemeLabels, I18n,
                                    termClause.ExpectedToken));
                            }

                            isError = isError || termRes.IsError;
                            break;
                        }
                        case NonTerminalClause<IN, OUT> nonTerminalClause:
                        {
                            var nonTerminalResult =
                                ParseNonTerminal(tokens, nonTerminalClause, currentPosition, parsingContext);
                            if (!nonTerminalResult.IsError)
                            {
                                errors.AddRange(nonTerminalResult.GetErrors());
                                children.Add(nonTerminalResult.Root);
                                currentPosition = nonTerminalResult.EndingPosition;
                            }
                            else
                            {
                                errors.AddRange(nonTerminalResult.GetErrors());
                            }

                            isError = isError || nonTerminalResult.IsError;
                            break;
                        }
                        case OneOrMoreClause<IN, OUT> _:
                        case ZeroOrMoreClause<IN, OUT> _:
                        case RepeatClause<IN, OUT> _:
                        {
                            SyntaxParseResult<IN, OUT> manyResult = null;
                            switch (clause)
                            {
                                case RepeatClause<IN,OUT> repeat:
                                    manyResult = ParseRepeat(tokens, repeat, currentPosition, parsingContext);
                                    break;
                                case OneOrMoreClause<IN, OUT> oneOrMore:
                                    manyResult = ParseOneOrMore(tokens, oneOrMore, currentPosition, parsingContext);
                                    break;
                                case ZeroOrMoreClause<IN, OUT> zeroOrMore:
                                    manyResult = ParseZeroOrMore(tokens, zeroOrMore, currentPosition, parsingContext);
                                    break;
                            }

                            if (!manyResult.IsError)
                            {
                                errors.AddRange(manyResult.GetErrors());
                                children.Add(manyResult.Root);
                                currentPosition = manyResult.EndingPosition;
                            }
                            else
                            {
                                if (manyResult.GetErrors() != null && manyResult.GetErrors().Count > 0)
                                    errors.AddRange(manyResult.GetErrors());
                            }

                            isError = manyResult.IsError;
                            break;
                        }
                        case OptionClause<IN, OUT> option:
                        {
                            var optionResult = ParseOption(tokens, option, rule, currentPosition, parsingContext);
                            currentPosition = optionResult.EndingPosition;
                            children.Add(optionResult.Root);
                            break;
                        }
                        case ChoiceClause<IN, OUT> choice:
                        {
                            var choiceResult = ParseChoice(tokens, choice, currentPosition, parsingContext);
                            currentPosition = choiceResult.EndingPosition;
                            if (choiceResult.IsError && choiceResult.GetErrors() != null && choiceResult.GetErrors().Any())
                            {
                                errors.AddRange(choiceResult.GetErrors());
                            }

                            isError = choiceResult.IsError;

                            children.Add(choiceResult.Root);
                            break;
                        }
                    }

                    if (isError) break;
                }
            }

            var result = new SyntaxParseResult<IN, OUT>();
            result.IsError = isError;
            result.AddErrors(errors);
            result.EndingPosition = currentPosition;
            if (!isError)
            {
                SyntaxNode<IN, OUT> node = null;
                if (rule.IsSubRule)
                {
                    node = new GroupSyntaxNode<IN, OUT>(nonTerminalName, children);
                    node = ManageExpressionRules(rule, node);
                    result.Root = node;
                    result.IsEnded = currentPosition >= tokens.Length - 1
                                     || currentPosition == tokens.Length - 2 &&
                                     tokens[tokens.Length - 1].IsEOS;
                }
                else
                {
                    if (rule.SubNodeNames != null && rule.SubNodeNames.Length > 0)
                    {
                        // Optimization: Use Math.Min to avoid bounds check
                        int maxIndex = Math.Min(rule.SubNodeNames.Length, children.Count);
                        for (int i = 0; i < maxIndex; i++)
                        {
                            var subNodeName = rule.SubNodeNames[i];
                            if (subNodeName != null)
                            {
                                var child = children[i];
                                child.ForceName(subNodeName);
                            }
                        }
                    }
                    node = new SyntaxNode<IN, OUT>( nonTerminalName,  children);
                    node.ForcedName = rule.ForcedName;
                    node.Name = string.IsNullOrEmpty(rule.NodeName) ? nonTerminalName : rule.NodeName;
                    node.ExpressionAffix = rule.ExpressionAffix;
                    node = ManageExpressionRules(rule, node);
                    result.Root = node;
                    result.IsEnded = tokens[result.EndingPosition].IsEOS
                                     || node.IsEpsilon && tokens[result.EndingPosition+1].IsEOS;  
                }
            }

            return result;
        }

        #endregion
    }
}