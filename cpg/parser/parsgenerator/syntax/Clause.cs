

namespace parser.parsergenerator.syntax
{

    public interface Clause<T>
    {
        bool Check(T nextToken);

    }
}