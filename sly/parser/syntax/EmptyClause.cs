namespace sly.parser.syntax
{

    public class EmptyClause<T> : IClause<T>
    {
        
        public bool Check(T nextToken) {
            return true;
        }

    }
}