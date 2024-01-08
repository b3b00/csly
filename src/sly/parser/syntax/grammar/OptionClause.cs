using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public sealed class OptionClause<T> : IClause<T>
    {
        public OptionClause(IClause<T> clause)
        {
            Clause = clause;
        }

        public IClause<T> Clause { get; set; }

        public bool IsGroupOption => Clause is NonTerminalClause<T> clause && clause.IsGroup;

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

        public bool Equals(IClause<T> clause)
        {
            if (clause is OptionClause<T> other)
            {
                return Equals(other);
            }
            return false;
        }

        private bool Equals(OptionClause<T> other)
        {
            return Equals(Clause, other.Clause);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OptionClause<T>)obj);
        }

        public override int GetHashCode()
        {
            return (Clause != null ? Clause.GetHashCode() : 0);
        }
    }
}