using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public sealed class OptionClause<IN,OUT> : IClause<IN,OUT> where IN : struct
    {
        public OptionClause(IClause<IN,OUT> clause)
        {
            Clause = clause;
        }

        public IClause<IN,OUT> Clause { get; set; }

        public bool IsGroupOption => Clause is NonTerminalClause<IN,OUT> clause && clause.IsGroup;

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

        public bool Equals(IClause<IN,OUT> clause)
        {
            if (clause is OptionClause<IN,OUT> other)
            {
                return Equals(other);
            }
            return false;
        }

        private bool Equals(OptionClause<IN,OUT> other)
        {
            return Equals(Clause, other.Clause);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OptionClause<IN,OUT>)obj);
        }

        public override int GetHashCode()
        {
            return (Clause != null ? Clause.GetHashCode() : 0);
        }
    }
}