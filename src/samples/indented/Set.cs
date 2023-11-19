namespace indented
{
    public class Set : Statement
    {
        public Identifier Id { get; set; }
        
        public Integer Value { get; set; }
        
        public string Comment { get; set; }

        public Set(Identifier id, Integer integer)
        {
            Id = id;
            Value = integer;
        }
        
        public string Dump(string tab)
        {
            return $"{tab}{Id.Dump(tab)} = {Value.Dump(tab)}";
        }
    }
}