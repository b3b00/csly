using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
    public class NonTerminal<IN> where IN : struct
    {
        public NonTerminal(string name, List<Rule<IN>> rules)
        {
            Name = name;
            Rules = rules;
        }

        public NonTerminal(string name) : this(name, new List<Rule<IN>>())
        {
        }

        public string Name { get; set; }

        public List<Rule<IN>> Rules { get; set; }

        public bool IsSubRule { get; set; }

        public List<LeadingToken<IN>> PossibleLeadingTokens => Rules.SelectMany<Rule<IN>, LeadingToken<IN>>(r => r.PossibleLeadingTokens).ToList<LeadingToken<IN>>();

        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            StringBuilder dump = new StringBuilder();
            
            foreach (var rule in Rules)
            {
                dump.Append(Name).Append(rule.IsExpressionRule ? " (R) ":"").Append(" : ");
                foreach (IClause<IN> clause in rule.Clauses)
                {
                    dump.Append(clause.Dump()).Append(" ");
                }
                dump.AppendLine();
            }

            return dump.ToString();
        }
    }
}