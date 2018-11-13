namespace sly.parser.syntax
{
    public class OneOrMoreClause<T> : ManyClause<T>
    {
        public OneOrMoreClause(IClause<T> clause)
        {
            Clause = clause;
        }


        public override string ToString()
        {
            return Clause + "+";
        }

        public override bool MayBeEmpty()
        {
            return true;
        }
    }
}