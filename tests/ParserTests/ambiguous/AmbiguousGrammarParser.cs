using sly.lexer;
using sly.parser.generator;

namespace ParserTests.ambiguous;

public class AmbiguousGrammarParser
{
    // S ::= 'a' S 'a'
    [Production("S : A S A")]
    public string Rule1(Token<AmbiguousToken> a1, string s, Token<AmbiguousToken> a2)
    {
        return $"aSa({s})";
    }

    // S ::= 'a' 'a' S
    [Production("S : A A S")]
    public string Rule2(Token<AmbiguousToken> a1, Token<AmbiguousToken> a2, string s)
    {
        return $"aaS({s})";
    }

    // S ::= 'a'
    [Production("S : A")]
    public string Rule3(Token<AmbiguousToken> a)
    {
        return "a";
    }
}