using System.Runtime.InteropServices.ComTypes;

namespace sly.parser.syntax
{

    public class OneOrMoreClause<T> : ManyClause<T>
    {
        public IClause<T> Clause { get; set; }
        public OneOrMoreClause(IClause<T> clause)
        {
            Clause = clause;
        }


        public override string ToString()
        {
            return Clause.ToString()+"+";
        }

        public override bool MayBeEmpty()
        {
            return true;
        }
    }
}