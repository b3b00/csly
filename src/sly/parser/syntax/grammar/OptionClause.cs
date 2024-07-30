using System.Diagnostics.CodeAnalysis;
using sly.lexer;
using sly.parser.syntax.tree;

namespace sly.parser.syntax.grammar
{
    public sealed class OptionClause<T> : IClause<T> where T : struct
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

        public bool IsTerminalOption()
        {
            return Clause is TerminalClause<T> || (Clause is OptionClause<T> option && option.IsTerminalOption());
        }

        public bool IsNonTerminalOption()
        {
            return Clause is NonTerminalClause<T> || (Clause is OptionClause<T> option && option.IsNonTerminalOption());
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