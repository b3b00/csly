

namespace sly.parser.syntax
{

    /// <summary>
    /// a clause within a grammar rule
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IClause<T>
    {
        bool Check(T nextToken);

    }
}