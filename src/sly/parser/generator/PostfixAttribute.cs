using System;

namespace sly.parser.generator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
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