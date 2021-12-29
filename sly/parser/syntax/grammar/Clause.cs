namespace sly.parser.syntax.grammar
{
    /// <summary>
    ///     a clause within a grammar rule
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IClause<T,OUT> : GrammarNode<T,OUT>
    {
        bool MayBeEmpty();
        string Dump();
    }
}