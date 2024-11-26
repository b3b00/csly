using System;

namespace sly.parser.generator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
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