using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace RelaxedVisitorTyping;

public class EbnfOptionRelaxedExpressionParser
{
    [Production("primary: Int")]
    public int primaryInt(Token<RelaxedExpressionToken> integer) => integer.IntValue;

    [Production("option : primary primary?")]
    public string Many(int first, ValueOption<int> second)
    {
        return second.Match((x) => $"{first}-{x}",() => $"{first}-NONE");
    }

}