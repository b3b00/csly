using System.Collections.Generic;
using System.Linq;
using sly.parser.syntax;

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

        public List<IN> PossibleLeadingTokens => Rules.SelectMany(r => r.PossibleLeadingTokens).ToList();
    }
}