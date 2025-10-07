using System.Text;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace RelaxedVisitorTyping;

public class EbnfRelaxedGroupErrorParser
{
    [Production("primary: Int")]
    public int primaryInt(Token<RelaxedExpressionToken> integer) => integer.IntValue;
    
    [Production("secondary : String")]
    public string primaryString(Token<RelaxedExpressionToken> str) => str.Value;

    [Production("group : primary (Property primary secondary)")]
    public string group(int first, Group<RelaxedExpressionToken, int> group)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(first);

        var p = group.Token(0).Value;
        var v = group.Value(1);
        var v2 = group.Value(2);
        builder.Append($" {p}={v} | {v2}");

        return builder.ToString();
    }
}