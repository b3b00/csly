using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace RelaxedVisitorTyping;

public class EbnfGroupRelaxedExpressionParser
{
    [Production("primary: Int")]
    public int primaryInt(Token<RelaxedExpressionToken> integer) => integer.IntValue;

    [Production("group : primary (Property primary)")]
    public string group(int first, Group<RelaxedExpressionToken, int> group)
    {
        var p = group.Token(0).Value;
        var v = group.Value(1);
        return $"{first} {p}={v}";
    }
}