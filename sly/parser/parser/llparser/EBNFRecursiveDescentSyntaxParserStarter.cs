using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.tree;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser
{
    public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT> : RecursiveDescentSyntaxParser<IN, OUT> where IN : struct
    {

        #region STARTING_TOKENS

        protected override void InitStartingTokensForRuleExtensions(IClause<IN,OUT> first, Rule<IN,OUT> rule,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            
            if (first is TerminalClause<IN,OUT>)
            {
                var term = first as TerminalClause<IN,OUT>;

                InitStartingTokensWithTerminal(rule, term);
            }
            else if (first is NonTerminalClause<IN,OUT>)
            {
                var nonterm = first as NonTerminalClause<IN,OUT>;
                InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
            }
            else if (first is ZeroOrMoreClause<IN,OUT> zeroOrMore)
            {
                InitStartingTokensWithZeroOrMore(rule, zeroOrMore, nonTerminals);
                int i = 1;
                bool optional = true;
                while (i < rule.Clauses.Count && optional)
                {
                    IClause<IN,OUT> clause = rule.Clauses[i];

                    switch (clause)
                    {
                        case TerminalClause<IN,OUT> terminalClause:
                        {
                            rule.PossibleLeadingTokens.Add(terminalClause.ExpectedToken);
                            break;
                        }
                        case NonTerminalClause<IN,OUT> terminalClause:
                        {
                            InitStartingTokensForNonTerminal(nonTerminals, terminalClause.NonTerminalName);
                            NonTerminal<IN,OUT> nonTerminal = nonTerminals[terminalClause.NonTerminalName];
                            {
                                rule.PossibleLeadingTokens.AddRange(nonTerminal.PossibleLeadingTokens);
                            }
                            break;
                        }
                        case ChoiceClause<IN,OUT> choice:
                        {
                            InitStartingTokensWithChoice(rule, choice, nonTerminals);
                            break;
                        }
                        case OptionClause<IN,OUT> option:
                        {
                            InitStartingTokensWithOption(rule, option, nonTerminals);
                            break;
                        }
                    }

                    // add startig tokens of clause in rule.startingtokens
                    optional = clause is ZeroOrMoreClause<IN,OUT> || clause is OptionClause<IN,OUT>;
                    i++;
                }
            }
            else if (first is OneOrMoreClause<IN,OUT>)
            {
                var many = first as OneOrMoreClause<IN,OUT>;
                InitStartingTokensWithOneOrMore(rule, many, nonTerminals);
            }
            else if (first is ChoiceClause<IN,OUT> choice)
            {
                InitStartingTokensWithChoice(rule, choice, nonTerminals);
            }
            else if (first is OptionClause<IN,OUT> option)
            {
               InitStartingTokensWithOption(rule,option,nonTerminals);
               int i = 1;
               bool optional = true;
               while (i < rule.Clauses.Count && optional)
               {
                   IClause<IN,OUT> clause = rule.Clauses[i];

                   switch (clause)
                   {
                       case TerminalClause<IN,OUT> terminalClause:
                       {
                           rule.PossibleLeadingTokens.Add(terminalClause.ExpectedToken);
                           break;
                       }
                       case NonTerminalClause<IN,OUT> terminalClause:
                       {
                           InitStartingTokensForNonTerminal(nonTerminals, terminalClause.NonTerminalName);
                           NonTerminal<IN,OUT> nonTerminal = nonTerminals[terminalClause.NonTerminalName];
                           {
                               rule.PossibleLeadingTokens.AddRange(nonTerminal.PossibleLeadingTokens);
                           }
                           break;
                       }
                       case ChoiceClause<IN,OUT> choiceClause:
                       {
                           InitStartingTokensWithChoice(rule, choiceClause, nonTerminals);
                           break;
                       }
                       case OptionClause<IN,OUT> optionClause:
                       {
                           InitStartingTokensWithOption(rule, optionClause, nonTerminals);
                           break;
                       }
                       case ZeroOrMoreClause<IN,OUT> zeroOrMoreClause:
                       {
                           InitStartingTokensWithZeroOrMore(rule,zeroOrMoreClause,nonTerminals);
                           break;
                       }
                       case OneOrMoreClause<IN,OUT> oneOrMoreClause:
                       {
                           InitStartingTokensWithOneOrMore(rule, oneOrMoreClause, nonTerminals);
                           break;
                       }
                    }

                   // add startig tokens of clause in rule.startingtokens
                   optional = clause is ZeroOrMoreClause<IN,OUT> || clause is OptionClause<IN,OUT>;
                   i++;
               }
            }
        }

        private void InitStartingTokensWithOption(Rule<IN,OUT> rule, OptionClause<IN,OUT> option,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            if (option.Clause is TerminalClause<IN,OUT> term)
            {
                InitStartingTokensWithTerminal(rule,term);
            }
            else if (option.Clause is NonTerminalClause<IN,OUT> nonTerminal)
            {
                InitStartingTokensWithNonTerminal(rule,nonTerminal,nonTerminals);
            }
            else if (option.Clause is ChoiceClause<IN,OUT> choice)
            {
                InitStartingTokensWithChoice(rule,choice,nonTerminals);
            }
        }

        private void InitStartingTokensWithChoice(Rule<IN,OUT> rule, ChoiceClause<IN,OUT> choice,Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            foreach (var alternate in choice.Choices)
            {
                if (alternate is TerminalClause<IN,OUT> term)
                {
                    InitStartingTokensWithTerminal(rule,term);
                }
                else if (alternate is NonTerminalClause<IN,OUT> nonTerminal)
                {
                    InitStartingTokensWithNonTerminal(rule,nonTerminal,nonTerminals);
                }
            }
        }


        private void InitStartingTokensWithTerminal(Rule<IN,OUT> rule, TerminalClause<IN,OUT> term)
        {
            rule.PossibleLeadingTokens.Add(term.ExpectedToken);
            rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
        }

        private void InitStartingTokensWithNonTerminal(Rule<IN,OUT> rule, NonTerminalClause<IN,OUT> nonterm,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            InitStartingTokensForNonTerminal(nonTerminals, nonterm.NonTerminalName);
            if (nonTerminals.ContainsKey(nonterm.NonTerminalName))
            {
                var firstNonTerminal = nonTerminals[nonterm.NonTerminalName];
                firstNonTerminal.Rules.ForEach(r => { rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens); });
                rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
            }
        }

        private void InitStartingTokensWithZeroOrMore(Rule<IN,OUT> rule, ZeroOrMoreClause<IN,OUT> manyClause,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            if (manyClause.Clause is TerminalClause<IN,OUT> term)
            {
                InitStartingTokensWithTerminal(rule, term);
            }
            else if (manyClause.Clause is NonTerminalClause<IN,OUT> nonTerm)
            {
                InitStartingTokensWithNonTerminal(rule, nonTerm, nonTerminals);
            }
            else if (manyClause.Clause is ChoiceClause<IN,OUT> choice)
            {
                InitStartingTokensWithChoice(rule,choice,nonTerminals);
            }
        }

        private void InitStartingTokensWithOneOrMore(Rule<IN,OUT> rule, OneOrMoreClause<IN,OUT> manyClause,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            if (manyClause.Clause is TerminalClause<IN,OUT> term)
            {
                InitStartingTokensWithTerminal(rule, term);
            }
            else if (manyClause.Clause is NonTerminalClause<IN,OUT> nonterm)
            {
                InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
            }
            else if (manyClause.Clause is ChoiceClause<IN,OUT> choice)
            {
                InitStartingTokensWithChoice(rule, choice, nonTerminals);
            }
        }

        #endregion

        
    }
}