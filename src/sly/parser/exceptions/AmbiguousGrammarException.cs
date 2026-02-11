using System;
using System.Text;
using sly.parser.syntax.tree;

namespace sly.parser.exceptions
{
    public class AmbiguousGrammarException<IN, OUT> : Exception where IN : struct, Enum
    {
        public ParseForest<IN, OUT> Forest { get; }
        
        public AmbiguousGrammarException(ParseForest<IN, OUT> forest)
            : base($"Ambiguous grammar detected: {forest.Count} alternative parse trees found")
        {
            Forest = forest;
        }
        
        public override string Message
        {
            get
            {
                var sb = new StringBuilder(base.Message);
                if (Forest.Ambiguities != null && Forest.Ambiguities.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Ambiguity points:");
                    foreach (var amb in Forest.Ambiguities)
                    {
                        sb.AppendLine($"  - NonTerminal '{amb.NonTerminalName}' at position {amb.Position}: {amb.AlternativeCount} alternatives");
                    }
                }
                return sb.ToString();
            }
        }
    }
}
