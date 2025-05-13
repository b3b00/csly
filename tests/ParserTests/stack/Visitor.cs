using System;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.stack;

public class Visitor : IDisposable {

    public void Dispose()
    {
        
        RuleParserType.ParserType = ParserType.LL_RECURSIVE_DESCENT;
    }
    
    [Production("root  : a [PLUS|MINUS] b")]
    public string Root(string a, Token<L> op, string b)
    {
        return "a <" + op.Value + "> b";
    }

    [Production("a : A")]
    public string A(Token<L> a)
    {
        return a.Value;
    }
    
    [Production("b : B")]
    public string B(Token<L> b)
    {
        return b.Value;
    }
}