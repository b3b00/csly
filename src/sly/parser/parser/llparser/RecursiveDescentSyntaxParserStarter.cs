using System.Collections.Generic;
using System.Linq;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser
{
    public partial class RecursiveDescentSyntaxParser<IN, OUT> : ISyntaxParser<IN, OUT> where IN : struct
    {
      

        public ParserConfiguration<IN, OUT> Configuration { get; set; }
        public string StartingNonTerminal { get; set; }
        
        public string I18n { get; set; }

          public ParserConfiguration<IN, OUT> ComputeSubRules(ParserConfiguration<IN, OUT> configuration)
        {
            var newNonTerms = new List<NonTerminal<IN>>();
            foreach (var nonTerm in configuration.NonTerminals)
            foreach (var rule in nonTerm.Value.Rules)
            {
                var newclauses = new List<IClause<IN>>();
                if (rule.ContainsSubRule)
                {
                    foreach (var clause in rule.Clauses)
                        switch (clause)
                        {
                            case GroupClause<IN> group:
                            {
                                var newNonTerm = CreateSubRule(group);
                                newNonTerms.Add(newNonTerm);
                                var newClause = new NonTerminalClause<IN>(newNonTerm.Name);
                                newClause.IsGroup = true;
                                newclauses.Add(newClause);
                                break;
                            }
                            case ManyClause<IN> many:
                            {
                                if (many.Clause is GroupClause<IN> manyGroup)
                                {
                                    var newNonTerm = CreateSubRule(manyGroup);
                                    newNonTerms.Add(newNonTerm);
                                    var newInnerNonTermClause = new NonTerminalClause<IN>(newNonTerm.Name);
                                    newInnerNonTermClause.IsGroup = true;
                                    many.Clause = newInnerNonTermClause;
                                    newclauses.Add(many);
                                }
                                else
                                {
                                    newclauses.Add(many);
                                }

                                break;
                            }
                            case OptionClause<IN> option:
                            {
                                if (option.Clause is GroupClause<IN> optionGroup)
                                {
                                    var newNonTerm = CreateSubRule(optionGroup);
                                    newNonTerms.Add(newNonTerm);
                                    var newInnerNonTermClause = new NonTerminalClause<IN>(newNonTerm.Name);
                                    newInnerNonTermClause.IsGroup = true;
                                    option.Clause = newInnerNonTermClause;
                                    newclauses.Add(option);
                                }
                                else
                                {
                                    newclauses.Add(option);
                                }

                                break;
                            }
                            default:
                                newclauses.Add(clause);
                                break;
                        }

                    rule.Clauses.Clear();
                    rule.Clauses.AddRange(newclauses);
                }
            }

            newNonTerms.ForEach(nonTerminal => configuration.AddNonTerminalIfNotExists(nonTerminal));
            return configuration;
        }

        public NonTerminal<IN> CreateSubRule(GroupClause<IN> group)
        {
            var clauses = string.Join("-",group.Clauses.Select(x => x.Dump())).Replace(" ","");
            var subRuleNonTerminalName = $"GROUP-{clauses}";
            var nonTerminal = new NonTerminal<IN>(subRuleNonTerminalName);
            var subRule = new Rule<IN>();
            subRule.Clauses = group.Clauses;
            subRule.IsSubRule = true;
            nonTerminal.Rules.Add(subRule);
            nonTerminal.IsSubRule = true;

            return nonTerminal;
        }

        #region STARTING_TOKENS

        protected virtual void InitializeStartingTokens(ParserConfiguration<IN, OUT> configuration, string root)
        {
            var nts = configuration.NonTerminals;


            InitStartingTokensForNonTerminal(nts, root);
            foreach (var nt in nts.Values)
            {
                foreach (var rule in nt.Rules)
                {
                    if (rule.PossibleLeadingTokens == null || rule.PossibleLeadingTokens.Count == 0)
                        InitStartingTokensForRule(nts, rule);
                }
            }
        }

        protected virtual void InitStartingTokensForNonTerminal(Dictionary<string, NonTerminal<IN>> nonTerminals,
            string name)
        {
            if (nonTerminals.TryGetValue(name, out var nt))
            {
                nt.Rules.ForEach(r => InitStartingTokensForRule(nonTerminals, r));
            }
        }

        protected void InitStartingTokensForRule(Dictionary<string, NonTerminal<IN>> nonTerminals,
            Rule<IN> rule)
        {
            if (rule.PossibleLeadingTokens != null && rule.PossibleLeadingTokens.Count != 0) return;
            rule.PossibleLeadingTokens = new List<LeadingToken<IN>>();
            if (rule.Clauses.Count <= 0) return;
            var first = rule.Clauses[0];
            switch (first)
            {
                case TerminalClause<IN> term:
                    rule.PossibleLeadingTokens.Add(term.ExpectedToken);
                    rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.Distinct().ToList();
                    break;
                case NonTerminalClause<IN> nonTerminalClause:
                {
                    InitStartingTokensForNonTerminal(nonTerminals, nonTerminalClause.NonTerminalName);
                    if (nonTerminals.TryGetValue(nonTerminalClause.NonTerminalName, out var firstNonTerminal))
                    {
                        firstNonTerminal.Rules.ForEach(r =>
                        {
                            rule.PossibleLeadingTokens.AddRange(r.PossibleLeadingTokens);
                        });
                        rule.PossibleLeadingTokens = rule.PossibleLeadingTokens.ToList();
                    }

                    break;
                }
                default:
                    InitStartingTokensForRuleExtensions(first,rule,nonTerminals);
                    break;
            }
        }

        protected virtual void InitStartingTokensForRuleExtensions(IClause<IN> first, Rule<IN> rule,
            Dictionary<string, NonTerminal<IN>> nonTerminals)
        {
        }

        #endregion


        public string Dump()
        {
            return this.Configuration.Dump();
        }
        
    }
}