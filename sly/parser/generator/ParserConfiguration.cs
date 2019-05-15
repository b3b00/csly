using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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