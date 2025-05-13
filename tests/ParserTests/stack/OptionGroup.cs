using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserTests.stack;

public class OptionGroup
{
    [Production("root : g")]
    public string Root(string g) => g;
    
    [Production("g : ( a b)? ")]
    public string G(ValueOption<Group<L,string>> optionalGroup)
    {
        return optionalGroup.Match(grp =>
            {
                return grp.Value(0)+" "+grp.Value(1);
            },
            () => "nothing");
    }

    [Production("a : A")]
    public string A(Token<L> a) => a.Value;
        
    [Production("b: B")]
    public string B(Token<L> b) => b.Value;
}