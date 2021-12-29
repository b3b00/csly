using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public class OneOrMoreClause<T,OUT> : ManyClause<T,OUT>
    {
        public OneOrMoreClause(IClause<T,OUT> clause)
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
    }
}