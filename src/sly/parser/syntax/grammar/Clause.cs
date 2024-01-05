using System;

namespace sly.parser.syntax.grammar
{
    /// <summary>
    ///     a clause within a grammar rule
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IClause<T> : GrammarNode<T>, IEquatable<IClause<T>>
    {
        bool MayBeEmpty();
        string Dump();
    }
}