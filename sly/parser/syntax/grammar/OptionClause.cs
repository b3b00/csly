using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public class OptionClause<T,OUT> : IClause<T,OUT>
    {
        public OptionClause(IClause<T,OUT> clause)
        {
            Clause = clause;
        }

        public IClause<T,OUT> Clause { get; set; }

        public bool IsGroupOption => Clause is NonTerminalClause<T,OUT> && (Clause as NonTerminalClause<T,OUT>).IsGroup;

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