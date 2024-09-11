namespace sly.parser.syntax.grammar
{
    public abstract class ManyClause<IN,OUT> : IClause<IN,OUT> where IN : struct
    {
        public IClause<IN,OUT> Clause { get; set; }

        public abstract bool MayBeEmpty();

        public abstract string Dump();
        public abstract bool Equals(IClause<IN,OUT> other);
    }
}