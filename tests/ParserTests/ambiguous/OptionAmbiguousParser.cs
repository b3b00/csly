using sly.lexer;
using sly.parser.generator;

namespace ParserTests.ambiguous;

public class OptionAmbiguousParser
{

    [Production("S : [a|b]")]
    public string RootOption(string option)
    {
        return $"S({option})";
    }


    [Production("a : A")]
    public string RuleA(Token<AmbiguousToken> a)
    {
        return "a(A)";
    }   
        
    [Production("b : A")]
    public string RuleB(Token<AmbiguousToken> a)
    {
        return "b(A)";
    }
}