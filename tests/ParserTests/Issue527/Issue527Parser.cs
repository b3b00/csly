using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue527;

public class Issue527Parser
{
    [Production("root : a* b+")]
    public string Root(List<string> a, List<string> b)
    {
        return $"any({string.Join(",", a)}) and boo({string.Join(",", b)})";
    }

    [Production("a: A")]
    public string A(Token<Issue527Lexer> a)
    {
        return "a";
    }
    
    [Production("b: B")]
    public string B(Token<Issue527Lexer> b)
    {
        return "b";
    }
}