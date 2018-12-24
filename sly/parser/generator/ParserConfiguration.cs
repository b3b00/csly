using System.Collections.Generic;

namespace sly.parser.generator
{
    public class ParserConfiguration<IN, OUT> where IN : struct
    {
        public string StartingRule { get; set; }
        public Dictionary<string, NonTerminal<IN>> NonTerminals { get; set; }


        public void AddNonTerminalIfNotExists(NonTerminal<IN> nonTerminal)
        {
            if (!NonTerminals.ContainsKey(nonTerminal.Name)) NonTerminals[nonTerminal.Name] = nonTerminal;
        }
    }
}