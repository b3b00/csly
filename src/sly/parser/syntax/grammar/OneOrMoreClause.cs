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

        
        private string _dump = null;
        
        [ExcludeFromCodeCoverage]
        public override string Dump()
        {
            if (_dump == null)
            {
                _dump = Clause.Dump()+"+";
            }

            return _dump;
        }

        [ExcludeFromCodeCoverage]
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