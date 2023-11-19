using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
    public class ParserConfiguration<IN, OUT> where IN : struct
    {
        public string StartingRule { get; set; }
        public Dictionary<string, NonTerminal<IN>> NonTerminals { get; set; }

        public bool UsesOperations { get; set; }

        public void AddNonTerminalIfNotExists(NonTerminal<IN> nonTerminal)
        {
            if (!NonTerminals.ContainsKey(nonTerminal.Name)) NonTerminals[nonTerminal.Name] = nonTerminal;
        }

        public bool HasExplicitTokens() => GetAllExplicitTokenClauses().Any();

        public List<TerminalClause<IN>> GetAllExplicitTokenClauses()
        {
            List<TerminalClause<IN>> clauses = new List<TerminalClause<IN>>();
            foreach (var nonTerminal in NonTerminals.Values)
            {
                foreach (var rule in nonTerminal.Rules)
                {
                    foreach (var clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<IN> terminalClause && terminalClause.IsExplicitToken)
                        {
                            clauses.Add(terminalClause);
                        }

                        if (clause is ChoiceClause<IN> choices)
                        {
                            foreach (var choice in choices.Choices)
                            {
                                if (choice is TerminalClause<IN> terminal && terminal.IsExplicitToken)
                                {
                                    clauses.Add(terminal);
                                }
                            }
                        }

                        if (clause is OptionClause<IN> option)
                        {
                            if (option.Clause is TerminalClause<IN> terminal && terminal.IsExplicitToken)
                                clauses.Add(terminal);
                        }
                    }
                }
            }

            return clauses;
        }

        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            StringBuilder dump = new StringBuilder();
            foreach (NonTerminal<IN> nonTerminal in NonTerminals.Values)
            {
                dump.AppendLine(nonTerminal.Dump());
            }

            return dump.ToString();
        }
    }
}