namespace indented
{
    public class Identifier : Ast
    {
        public string Comment { get; set; }
        public string Name { get; set; }
        
        public Identifier(string name)
        {
            Name = name;
        }

        public string Dump(string tab)
        {
            return Name;
        }
    }
}