using System.Collections.Generic;
using System.Reflection;

namespace sly.parser.generator
{
    public class ParserConfiguration<IN,OUT> where IN : struct
    {
        public Dictionary<string, MethodInfo> Functions { get; set; }
        public Dictionary<string, NonTerminal<IN>> NonTerminals { get; set; }
    }
}
