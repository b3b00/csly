using System.Collections.Generic;
using System.Linq;
using System.Text;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue443;

[ParserRoot("root")]
public class Test443Parser
{
    [Production("root : a b* a")]
    public string Root(string a1, List<string> bees, string a2)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(a1?.ToString());
        if (bees != null && bees.Any())
        {
            foreach (object bee in bees)
            {
                builder.Append(bee?.ToString());
            }
        }
        builder.Append(a2?.ToString());
        return builder.ToString();
    }
    
    [Production("a : A")]
    public string A(Token<Test443Lexer> a)
    {
        return a.Value;
    }
    
    [Production("b : B")]
    public string B(Token<Test443Lexer> b)
    {
        return b.Value;
    }
}