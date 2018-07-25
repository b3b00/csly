namespace sly.parser.syntax
{

    public class NonTerminalClause<T> : IClause<T>
    {
        public string NonTerminalName { get; set; }

        public bool IsGroup { get; set; } = false;
        public NonTerminalClause(string name)
        {
            NonTerminalName = name;
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