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
        public Dictionary<string, NonTerminal<IN,OUT>> NonTerminals { get; set; }

        public bool UsesOperations { get; set; }

        public bool UseMemoization { get; set; } = false;

        public bool BroadenTokenWindow { get; set; } = false;
        
        public bool AutoCloseIndentations { get; set; } 

        public void AddNonTerminalIfNotExists(NonTerminal<IN,OUT> nonTerminal)
        {
            if (!NonTerminals.ContainsKey(nonTerminal.Name)) NonTerminals[nonTerminal.Name] = nonTerminal;
        }

        public bool HasExplicitTokens() => GetAllExplicitTokenClauses().Any();

        public List<TerminalClause<IN,OUT>> GetAllExplicitTokenClauses()
        {
            List<TerminalClause<IN,OUT>> clauses = new List<TerminalClause<IN,OUT>>();
            foreach (var nonTerminal in NonTerminals.Values)
            {
                foreach (var rule in nonTerminal.Rules)
                {
                    foreach (var clause in rule.Clauses)
                    {
                        if (clause is TerminalClause<IN,OUT> terminalClause && terminalClause.IsExplicitToken)
                        {
                            clauses.Add(terminalClause);
                        }

                        if (clause is ChoiceClause<IN,OUT> choices)
                        {
                            foreach (var choice in choices.Choices)
                            {
                                if (choice is TerminalClause<IN,OUT> terminal && terminal.IsExplicitToken)
                                {
                                    clauses.Add(terminal);
                                }
                            }
                        }

                        if (clause is not OptionClause<IN,OUT> option) continue;
                        {
                            if (option.Clause is TerminalClause<IN,OUT> { IsExplicitToken: true } terminal)
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
            foreach (NonTerminal<IN,OUT> nonTerminal in NonTerminals.Values)
            {
                dump.AppendLine(nonTerminal.Dump());
            }

            return dump.ToString();
        }
    }
}