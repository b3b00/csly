using System.Diagnostics.CodeAnalysis;

namespace sly.parser.syntax.grammar
{
    public class ZeroOrMoreClause<T,OUT> : ManyClause<T,OUT>
    {
        
        public ZeroOrMoreClause(IClause<T,OUT> clause)
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
            return Clause.Dump()+"*";
        }
    }
}