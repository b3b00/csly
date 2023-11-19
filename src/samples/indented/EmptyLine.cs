namespace indented
{
    public class EmptyLine : Ast
    {
        
        public string Comment { get; set; }
        public EmptyLine()
        {
        }

        public string Dump(string tab)
        {
            return "";
        }
    }
}