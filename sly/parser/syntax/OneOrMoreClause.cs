namespace sly.parser.syntax
{

    public class OneOrMoreClause<T> : IClause<T>
    {
        public string ClauseName { get; set; }
        public OneOrMoreClause(string name)
        {
            ClauseName = name;
        }
        public bool Check(T nextToken)
        {
            return true;
        }

    }
}