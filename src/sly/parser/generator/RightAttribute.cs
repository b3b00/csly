namespace sly.parser.generator;

public class RightAttribute : InfixAttribute
{
    public RightAttribute(int intToken,  int precedence) : base(intToken,Associativity.Right,precedence)
    {
    }
        
    public RightAttribute(string stringToken,  int precedence) : base(stringToken,Associativity.Right, precedence)
    {
    }
}