namespace sly.parser.generator
{
    public class PrefixAttribute : OperationAttribute
    {
        public PrefixAttribute(int intToken,  Associativity assoc, int precedence) : base(intToken,Affix.PreFix,assoc,precedence)
        {
        }
        
        public PrefixAttribute(string stringToken,  Associativity assoc, int precedence) : base(stringToken,Affix.PreFix, assoc,precedence)
        {
        }
    }
}