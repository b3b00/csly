using System.Collections.Generic;
using System.Linq;
using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.llparser.bnf;

namespace sly.parser.llparser.ebnf
{
    public partial class EBNFRecursiveDescentSyntaxParser<IN, OUT> where IN : struct
    {

        #region STARTING_TOKENS

        protected override void InitStartingTokensForRuleExtensions(IClause<IN,OUT> first, Rule<IN,OUT> rule,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            switch (first)
            {
                case ZeroOrMoreClause<IN,OUT> zeroOrMore:
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
                            case NonTerminalClause<IN,OUT> nonTerminalClause:
                            {
                                InitStartingTokensForNonTerminal(nonTerminals, nonTerminalClause.NonTerminalName);
                                NonTerminal<IN,OUT> nonTerminal = nonTerminals[nonTerminalClause.NonTerminalName];
                                {
                                    rule.PossibleLeadingTokens.AddRange(nonTerminal.GetPossibleLeadingTokens());
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

                    break;
                }
                case OneOrMoreClause<IN,OUT> clause:
                {
                    var many = clause;
                    InitStartingTokensWithOneOrMore(rule, many, nonTerminals);
                    break;
                }
                case ChoiceClause<IN,OUT> choice:
                    InitStartingTokensWithChoice(rule, choice, nonTerminals);
                    break;
                case OptionClause<IN,OUT> option:
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
                                    rule.PossibleLeadingTokens.AddRange(nonTerminal.GetPossibleLeadingTokens());
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

                    break;
                }
            }
        }

        private void InitStartingTokensWithOption(Rule<IN,OUT> rule, OptionClause<IN,OUT> option,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            switch (option.Clause)
            {
                case TerminalClause<IN,OUT> term:
                    InitStartingTokensWithTerminal(rule,term);
                    break;
                case NonTerminalClause<IN,OUT> nonTerminal:
                    InitStartingTokensWithNonTerminal(rule,nonTerminal,nonTerminals);
                    break;
                case ChoiceClause<IN,OUT> choice:
                    InitStartingTokensWithChoice(rule,choice,nonTerminals);
                    break;
            }
        }

        private void InitStartingTokensWithChoice(Rule<IN,OUT> rule, ChoiceClause<IN,OUT> choice,Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            foreach (var alternate in choice.Choices)
            {
                switch (alternate)
                {
                    case TerminalClause<IN,OUT> term:
                        InitStartingTokensWithTerminal(rule,term);
                        break;
                    case NonTerminalClause<IN,OUT> nonTerminal:
                        InitStartingTokensWithNonTerminal(rule,nonTerminal,nonTerminals);
                        break;
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
            if (nonTerminals.TryGetValue(nonterm.NonTerminalName, out var firstNonTerminal))
            {
                firstNonTerminal.Rules.ForEach(r => { rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens); });
                rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
            }
        }

        private void InitStartingTokensWithZeroOrMore(Rule<IN,OUT> rule, ZeroOrMoreClause<IN,OUT> manyClause,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            switch (manyClause.Clause)
            {
                case TerminalClause<IN,OUT> term:
                    InitStartingTokensWithTerminal(rule, term);
                    break;
                case NonTerminalClause<IN,OUT> nonTerm:
                    InitStartingTokensWithNonTerminal(rule, nonTerm, nonTerminals);
                    break;
                case ChoiceClause<IN,OUT> choice:
                    InitStartingTokensWithChoice(rule,choice,nonTerminals);
                    break;
            }
        }

        private void InitStartingTokensWithOneOrMore(Rule<IN, OUT> rule, OneOrMoreClause<IN,OUT> manyClause,
            Dictionary<string, NonTerminal<IN,OUT>> nonTerminals)
        {
            switch (manyClause.Clause)
            {
                case TerminalClause<IN,OUT> term:
                    InitStartingTokensWithTerminal(rule, term);
                    break;
                case NonTerminalClause<IN,OUT> nonterm:
                    InitStartingTokensWithNonTerminal(rule, nonterm, nonTerminals);
                    break;
                case ChoiceClause<IN,OUT> choice:
                    InitStartingTokensWithChoice(rule, choice, nonTerminals);
                    break;
            }
        }

        #endregion

        
    }
}