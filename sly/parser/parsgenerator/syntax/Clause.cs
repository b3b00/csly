

namespace sly.parser.syntax
{

    public interface Clause<T>
    {
        bool Check(T nextToken);

    }
}