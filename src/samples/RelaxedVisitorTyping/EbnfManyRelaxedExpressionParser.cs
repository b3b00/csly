using sly.lexer;
using sly.parser.generator;

namespace RelaxedVisitorTyping;

public class EbnfManyRelaxedExpressionParser
{
    [Production("primary: Int")]
    public int primaryInt(Token<RelaxedExpressionToken> integer) => integer.IntValue;

    [Production("many : primary*")]
    public List<int> Many(List<int> integers) => integers;

}