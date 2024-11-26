using System;

namespace sly.parser.generator
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class InfixAttribute : OperationAttribute
    {
        public InfixAttribute(int intToken,  Associativity assoc, int precedence) : base(intToken,Affix.InFix,assoc,precedence)
        {
        }
        
        public InfixAttribute(string stringToken,  Associativity assoc, int precedence) : base(stringToken,Affix.InFix, assoc,precedence)
        {
        }
    }
}