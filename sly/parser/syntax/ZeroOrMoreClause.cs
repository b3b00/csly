namespace sly.parser.syntax
{
    public class ZeroOrMoreClause<T> : ManyClause<T>
    {
        public ZeroOrMoreClause(IClause<T> clause)
        {
            Clause = clause;
        }

        public override string ToString()
        {
            return Clause + "*";
        }

        public override bool MayBeEmpty()
        {
            return true;
        }
        
        public override string Dump()
        {
            return Clause.Dump()+"*";
        }
    }
}