namespace indented
{
    public class Set : Statement
    {
        public Identifier Id { get; set; }
        
        public Integer Value { get; set; }

        public Set(Identifier id, Integer integer)
        {
            Id = id;
            Value = integer;
        }
        
        public string Dump(string tab)
        {
            return $"{Id.Dump(tab)} = {Value.Dump(tab)}";
        }
    }
}