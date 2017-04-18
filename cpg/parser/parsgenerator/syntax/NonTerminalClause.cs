namespace parser.parsergenerator.syntax
{

    public class NonTerminalClause<T> : Clause<T>
    {
        private string NonTerminalName { get; set; }
        public NonTerminalClause(string name)
        {
            NonTerminalName = name;
        }
        public object Check(T nextToken)
        {
            return null;
        }

    }
}