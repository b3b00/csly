using System;

namespace sly.parser.generator;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class LeftAttribute : InfixAttribute
{
    public LeftAttribute(int intToken,  int precedence) : base(intToken,Associativity.Left,precedence)
    {
    }
        
    public LeftAttribute(string stringToken,  int precedence) : base(stringToken,Associativity.Left, precedence)
    {
    }
}