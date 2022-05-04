namespace indented
{
    public interface Ast
    {
        
        public string Comment { get; set; }
        public string Dump(string tab);
    }
}