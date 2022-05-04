namespace indented
{
    public class Cond : Ast
    {
        public Identifier Id { get; set; }
        
        public Integer Value { get; set; }
        
        public string Comment { get; set; }

        public Cond(Identifier id, Integer integer)
        {
            Id = id;
            Value = integer;
        }

        public string Dump(string tab)
        {
            return $"{Id.Dump(tab)} == {Value.Dump(tab)}";
        }
    }
}