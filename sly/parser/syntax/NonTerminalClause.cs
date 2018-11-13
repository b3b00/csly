namespace sly.parser.syntax
{
    public class NonTerminalClause<T> : IClause<T>
    {
        public NonTerminalClause(string name)
        {
            NonTerminalName = name;
        }

        public string NonTerminalName { get; set; }

        public bool IsGroup { get; set; } = false;

        public bool MayBeEmpty()
        {
            return false;
        }

        public override string ToString()
        {
            return NonTerminalName;
        }
    }
}