using System;

namespace sly.parser.syntax.grammar
{
    /// <summary>
    ///     a clause within a grammar rule
    /// </summary>
    /// <typeparam name="IN"></typeparam>
    public interface IClause<IN,OUT> : GrammarNode<IN,OUT>, IEquatable<IClause<IN,OUT>> where IN : struct
    {
        bool MayBeEmpty();
        string Dump();
    }
}