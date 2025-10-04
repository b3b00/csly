using System.Text;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace RelaxedVisitorTyping;

public class EbnfGroupRelaxedExpressionParser
{
    [Production("primary: Int")]
    public int primaryInt(Token<RelaxedExpressionToken> integer) => integer.IntValue;

    [Production("group : primary (Property primary)+ (Property Property)?")]
    public string group(int first, List<Group<RelaxedExpressionToken, int>> groups, ValueOption<Group<RelaxedExpressionToken, int>> option)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(first);
        foreach (var group in groups)
        {
            var p = group.Token(0).Value;
            var v = group.Value(1);
            builder.Append($" {p}={v}");
        }
        if (option.IsSome)
        {
            var optionValue = option.Match((x) =>
            {
                return $"{x.Token(0).TokenID}={x.Token(1).TokenID}";
            },
                () => "no option");
            builder.Append(" - ").Append(optionValue);
        }
        return builder.ToString();
    }
}