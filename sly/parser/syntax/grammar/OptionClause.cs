using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public class OptionClause<T> : IClause<T>
    {
        public OptionClause(IClause<T> clause)
        {
            Clause = clause;
        }

        public IClause<T> Clause { get; set; }

        public bool IsGroupOption => Clause is NonTerminalClause<T> && (Clause as NonTerminalClause<T>).IsGroup;

        public bool MayBeEmpty()
        {
            return true;
        }

        
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{Clause}?";
        }
        
        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return $"{Clause}?";
        }
    }
}