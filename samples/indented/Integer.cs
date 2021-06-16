namespace indented
{
    public class Integer : Ast
    {
        public int Value { get; set; }

        public Integer(int value)
        {
            Value = value;
        }

        public string Dump(string tab)
        {
            return Value.ToString();
        }
    }
}