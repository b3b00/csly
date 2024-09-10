using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public sealed class ZeroOrMoreClause<IN,OUT> : ManyClause<IN,OUT> where IN : struct
    {
        
        public ZeroOrMoreClause(IClause<IN,OUT> clause)
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

        public override bool Equals(IClause<IN,OUT> other)
        {
            if (other is OneOrMoreClause<IN,OUT> otherOneOrMore)
            {
                return Clause.Equals(otherOneOrMore.Clause);
            }
            return false;
        }
    }
}