using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.tree;
using sly.parser.syntax.grammar;
using sly.parser.llparser.bnf;

namespace sly.parser.llparser.ebnf
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
            if (rule.Match(tokens, position, Configuration) && rule.Clauses != null && rule.Clauses.Count > 0)
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
                                errors.Add(new UnexpectedTokenSyntaxError<IN>(tok, LexemeLabels, I18n,
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

            var result = new SyntaxParseResult<IN>();
            result.IsError = isError;
            result.AddErrors(errors);
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