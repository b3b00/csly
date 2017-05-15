using System.Runtime.InteropServices.ComTypes;

namespace sly.parser.syntax
{

    public class OneOrMoreClause<T> : IClause<T>
    {
        public IClause<T> Clause { get; set; }
        public OneOrMoreClause(IClause<T> clause)
        {
            Clause = clause;
        }
        public bool Check(T nextToken)
        {
            return true;
        }


        public override string ToString()
        {
            return Clause.ToString()+"+";
        }
    }
}