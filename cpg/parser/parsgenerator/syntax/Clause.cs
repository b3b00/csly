

namespace parser.parsergenerator.syntax
{

    public interface Clause<T>
    {
        object Check(T nextToken);

    }
}