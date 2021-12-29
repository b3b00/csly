namespace sly.parser.syntax.grammar
{
    public abstract class ManyClause<T,OUT> : IClause<T,OUT>
    {
        public IClause<T,OUT> Clause { get; set; }

        public abstract bool MayBeEmpty();

        public abstract string Dump();
    }
}