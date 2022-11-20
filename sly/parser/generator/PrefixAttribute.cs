namespace sly.parser.generator
{
    public class PrefixAttribute<IN> : OperationAttribute<IN> where IN : struct
    {
        public PrefixAttribute(int intToken,  Associativity assoc, int precedence) : base(intToken,Affix.PreFix,assoc,precedence)
        {
        }
        
        public PrefixAttribute(string stringToken,  Associativity assoc, int precedence) : base(stringToken,Affix.PreFix, assoc,precedence)
        {
        }
        
        public PrefixAttribute(IN tokenID,   Associativity assoc, int precedence) : base(tokenID,Affix.PreFix, assoc,precedence)
        {
        }
    }
}