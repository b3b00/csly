namespace sly.parser.syntax
{

    public class OptionClause<T> : IClause<T>
    {
        public IClause<T> Clause { get; set; }
        public OptionClause(IClause<T> clause)
        {
            Clause = clause;
        }

        public override string ToString()
        {
            return $"{Clause.ToString()}?";
        }

        public bool MayBeEmpty()
        {
            return true;
        }

    }
}