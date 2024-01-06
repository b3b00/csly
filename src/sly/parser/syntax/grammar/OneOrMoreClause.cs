using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public sealed class OneOrMoreClause<T> : ManyClause<T>
    {
        public OneOrMoreClause(IClause<T> clause)
        {
            Clause = clause;
        }


        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Clause + "+";
        }

        public override bool MayBeEmpty()
        {
            return true;
        }
        
        [ExcludeFromCodeCoverage]
        public override string Dump()
        {
            return Clause.Dump()+"+";
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