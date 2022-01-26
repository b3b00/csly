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
                        case OptionClause<IN> option:
                        {
                            InitStartingTokensWithOption(rule, option, nonTerminals);
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
            else if (first is OptionClause<IN> option)
            {
               InitStartingTokensWithOption(rule,option,nonTerminals);
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
                       case ChoiceClause<IN> choiceClause:
                       {
                           InitStartingTokensWithChoice(rule, choiceClause, nonTerminals);
                           break;
                       }
                       case OptionClause<IN> optionClause:
                       {
                           InitStartingTokensWithOption(rule, optionClause, nonTerminals);
                           break;
                       }
                       case ZeroOrMoreClause<IN> zeroOrMoreClause:
                       {
                           InitStartingTokensWithZeroOrMore(rule,zeroOrMoreClause,nonTerminals);
                           break;
                       }
                       case OneOrMoreClause<IN> oneOrMoreClause:
                       {
                           InitStartingTokensWithOneOrMore(rule, oneOrMoreClause, nonTerminals);
                           break;
                       }
                    }

                   // add startig tokens of clause in rule.startingtokens
                   optional = clause is ZeroOrMoreClause<IN> || clause is OptionClause<IN>;
                   i++;
               }
            }
        }

        private void InitStartingTokensWithOption(Rule<IN> rule, OptionClause<IN> option,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            if (option.Clause is TerminalClause<IN> term)
            {
                InitStartingTokensWithTerminal(rule,term);
            }
            else if (option.Clause is NonTerminalClause<IN> nonTerminal)
            {
                InitStartingTokensWithNonTerminal(rule,nonTerminal,nonTerminals);
            }
            else if (option.Clause is ChoiceClause<IN> choice)
            {
                InitStartingTokensWithChoice(rule,choice,nonTerminals);
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

        
    }
}