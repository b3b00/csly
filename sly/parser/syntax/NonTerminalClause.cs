namespace sly.parser.syntax
{

    public class NonTerminalClause<T> : IClause<T>
    {
        public string NonTerminalName { get; set; }
        public NonTerminalClause(string name)
        {
            NonTerminalName = name;
        }
        public bool Check(T nextToken)
        {
            return true;
        }

        public override string ToString()
        {
            return NonTerminalName;
        }

        public bool MayBeEmpty()
        {
            return false;
        }

    }
}