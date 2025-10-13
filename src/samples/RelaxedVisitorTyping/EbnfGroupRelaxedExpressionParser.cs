using System.Text;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace RelaxedVisitorTyping;

public class EbnfGroupRelaxedExpressionParser
{
    [Production("primary: Int")]
    public int primaryInt(Token<RelaxedExpressionToken> integer) => integer.IntValue;

    // [Production("primstring: String")]
    // public string primaryString(Token<RelaxedExpressionToken> stringLiteral) => stringLiteral.Value;

    [Production("manyString: String*")]
    public string ManyT(List<Token<RelaxedExpressionToken>> l) => string.Join(",",l.Select(x => x.StringWithoutQuotes));
    
    [Production("primaryOpt: primary?")]
    public string ManyNT(ValueOption<int> l) => l.Match(x=> x.ToString(), () => "nothing" );
    
    
    
    [Production("group : primary (Property primary)+  (Property primary)? primaryOpt manyString")]
    public string group(int first, List<Group<RelaxedExpressionToken, int>> groups,
         ValueOption<Group<RelaxedExpressionToken, int>> optionOk, string primOpt, string manyString)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(first);
        foreach (var group in groups)
        {
            var p = group.Token(0).Value;
            var v = group.Value(1);
            builder.Append($" {p}={v}");
        }
        

        var optionValue = optionOk.Match((x) => { return $"{x.Token(0).Value}={x.Token(1).Value}"; },
            () => "no option");
        builder.Append(" - ").Append(optionValue);
        builder.Append(" - ").Append(manyString);
        builder.Append(" - ").Append(primOpt);
        return builder.ToString();
    }
}