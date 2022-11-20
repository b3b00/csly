namespace sly.parser.generator
{
    public class PostfixAttribute<IN>: OperationAttribute<IN>
    {
        public PostfixAttribute(int intToken,  Associativity assoc, int precedence) : base(intToken,Affix.PostFix,assoc,precedence)
        {
        }
        
        public PostfixAttribute(string stringToken,  Associativity assoc, int precedence) : base(stringToken,Affix.PostFix, assoc,precedence)
        {
        }
        
        public PostfixAttribute(IN tokenID,  Associativity assoc, int precedence) : base(tokenID,Affix.PostFix, assoc,precedence)
        {
        }
    }
}