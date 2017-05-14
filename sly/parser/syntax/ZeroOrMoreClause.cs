namespace sly.parser.syntax
{

    public class ZeroOrMoreClause<T> : IClause<T>
    {
        public string ClauseName { get; set; }
        public ZeroOrMoreClause(string name)
        {
            ClauseName = name;
        }
        public bool Check(T nextToken)
        {
            return true;
        }

    }
}