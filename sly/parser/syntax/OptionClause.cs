namespace sly.parser.syntax
{
    public class OptionClause<T> : IClause<T>
    {
        public OptionClause(IClause<T> clause)
        {
            Clause = clause;
        }

        public IClause<T> Clause { get; set; }

        public bool IsGroupOption => Clause is NonTerminalClause<T> && (Clause as NonTerminalClause<T>).IsGroup;

        public bool MayBeEmpty()
        {
            return true;
        }

        
        public override string ToString()
        {
            return $"{Clause}?";
        }
        
        public string Dump()
        {
            return $"{Clause}?";
        }
    }
}