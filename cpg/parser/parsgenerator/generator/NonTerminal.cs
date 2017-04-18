using System;
using System.Collections.Generic;
using System.Text;

namespace cpg.parser.parsgenerator.generator
{
    public class NonTerminal<T>
    {

        public string Name { get; set; }

        public List<Rule<T>> Rules { get; set; }

        public NonTerminal(string name, List<Rule<T>> rules)
        {
            Name = name;
            Rules = rules;
        }
    }
}
