using System.Collections.Generic;
using System.Linq;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser
{
    public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT> where IN : struct
    {

        #region STARTING_TOKENS

        protected override void InitStartingTokensForRuleExtensions(IClause<IN> first, Rule<IN> rule,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            switch (first)
            {
                case ZeroOrMoreClause<IN> zeroOrMore:
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
                            case NonTerminalClause<IN> nonTerminalClause:
                            {
                                InitStartingTokensForNonTerminal(nonTerminals, nonTerminalClause.NonTerminalName);
                                NonTerminal<IN> nonTerminal = nonTerminals[nonTerminalClause.NonTerminalName];
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

                    break;
                }
                case OneOrMoreClause<IN> clause:
                {
                    var many = clause;
                    InitStartingTokensWithOneOrMore(rule, many, nonTerminals);
                    break;
                }
                case ChoiceClause<IN> choice:
                    InitStartingTokensWithChoice(rule, choice, nonTerminals);
                    break;
                case OptionClause<IN> option:
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

                    break;
                }
            }
        }

        private void InitStartingTokensWithOption(Rule<IN> rule, OptionClause<IN> option,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            switch (option.Clause)
            {
                case TerminalClause<IN> term:
                    InitStartingTokensWithTerminal(rule,term);
                    break;
                case NonTerminalClause<IN> nonTerminal:
                    InitStartingTokensWithNonTerminal(rule,nonTerminal,nonTerminals);
                    break;
                case ChoiceClause<IN> choice:
                    InitStartingTokensWithChoice(rule,choice,nonTerminals);
                    break;
            }
        }

        private void InitStartingTokensWithChoice(Rule<IN> rule, ChoiceClause<IN> choice,Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            foreach (var alternate in choice.Choices)
            {
                switch (alternate)
                {
                    case TerminalClause<IN> term:
                        InitStartingTokensWithTerminal(rule,term);
                        break;
                    case NonTerminalClause<IN> nonTerminal:
                        InitStartingTokensWithNonTerminal(rule,nonTerminal,nonTerminals);
                        break;
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
            if (nonTerminals.TryGetValue(nonterm.NonTerminalName, out var firstNonTerminal))
            {
                firstNonTerminal.Rules.ForEach(r => { rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens); });
                rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
            }
        }

        private void InitStartingTokensWithZeroOrMore(Rule<IN> rule, ZeroOrMoreClause<IN> manyClause,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            switch (manyClause.Clause)
            {
                case TerminalClause<IN> term:
                    InitStartingTokensWithTerminal(rule, term);
                    break;
                case NonTerminalClause<IN> nonTerm:
                    InitStartingTokensWithNonTerminal(rule, nonTerm, nonTerminals);
                    break;
                case ChoiceClause<IN> choice:
                    InitStartingTokensWithChoice(rule,choice,nonTerminals);
                    break;
            }
        }

        private void InitStartingTokensWithOneOrMore(Rule<IN> rule, OneOrMoreClause<IN> manyClause,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
            switch (manyClause.Clause)
            {
                case TerminalClause<IN> term:
                    InitStartingTokensWithTerminal(rule, term);
                    break;
                case NonTerminalClause<IN> nonterm:
                    InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
                    break;
                case ChoiceClause<IN> choice:
                    InitStartingTokensWithChoice(rule, choice, nonTerminals);
                    break;
            }
        }

        #endregion

        
    }
}