namespace parser.parsergenerator.syntax
{

    public class NonTerminalClause<T> : Clause<T>
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

    }
}