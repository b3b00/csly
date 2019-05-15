using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public class NonTerminalClause<T> : IClause<T>
    {
        public NonTerminalClause(string name)
        {
            NonTerminalName = name;
        }

        public string NonTerminalName { get; set; }

        public bool IsGroup { get; set; } = false;

        public bool MayBeEmpty()
        {
            return false;
        }

        
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return NonTerminalName;
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return NonTerminalName;
        }
    }
}