using System;
using System.Collections.Generic;
using sly.parser.syntax;


namespace sly.parser.generator
{
    public class NonTerminal<IN>
    {

        public string Name { get; set; }

        public List<Rule<IN>> Rules { get; set; }

        public NonTerminal(string name, List<Rule<IN>> rules)
        {
            Name = name;
            Rules = rules;
        }
    }
}
