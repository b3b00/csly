namespace sly.parser.generator
{
    public class InfixAttribute<IN> : OperationAttribute<IN>
    {
        public InfixAttribute(int intToken,  Associativity assoc, int precedence) : base(intToken,Affix.InFix,assoc,precedence)
        {
        }
        
        public InfixAttribute(string stringToken,  Associativity assoc, int precedence) : base(stringToken,Affix.InFix, assoc,precedence)
        {
        }
        
        public InfixAttribute(IN tokenID,  Associativity assoc, int precedence) : base(tokenID,Affix.InFix, assoc,precedence)
        {
        }
    }
}