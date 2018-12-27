namespace sly.parser.syntax
{
    public abstract class ManyClause<T> : IClause<T>
    {
        public IClause<T> Clause { get; set; }

        public abstract bool MayBeEmpty();

        public abstract string Dump();
    }
}