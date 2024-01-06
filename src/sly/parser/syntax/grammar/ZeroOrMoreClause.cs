using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public sealed class ZeroOrMoreClause<T> : ManyClause<T>
    {
        
        public ZeroOrMoreClause(IClause<T> clause)
        {
            Clause = clause;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Clause + "*";
        }

        public override bool MayBeEmpty()
        {
            return true;
        }
        
        [ExcludeFromCodeCoverage]
        public override string Dump()
        {
            var t = Clause.Dump() + "*";
            return t;
        }

        public override bool Equals(IClause<T> other)
        {
            if (other is OneOrMoreClause<T> otherOneOrMore)
            {
                return Clause.Equals(otherOneOrMore.Clause);
            }
            return false;
        }
    }
}