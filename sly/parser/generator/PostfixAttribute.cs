namespace sly.parser.generator
{
    public class PostfixAttribute : OperationAttribute
    {
        public PostfixAttribute(int intToken,  Associativity assoc, int precedence) : base(intToken,Affix.PostFix,assoc,precedence)
        {
        }
        
        public PostfixAttribute(string stringToken,  Associativity assoc, int precedence) : base(stringToken,Affix.PostFix, assoc,precedence)
        {
        }
    }
}