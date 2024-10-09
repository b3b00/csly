using System.Collections.Generic;
using System.Linq;

namespace sly.sourceGenerator.generated.ebnfparser.model;

public class AlternateClause : IClause
{
    public IList<IClause> Choices { get; set; }

    public bool IsTerminalAlternate => Choices != null && Choices.Count > 1 && Choices[0] is TerminalClause;
    
    public bool IsNonTerminalAlternate => Choices != null && Choices.Count > 1 && Choices[0] is NonTerminalClause;
    public AlternateClause(IList<IClause> choices)
    {
        Choices = choices;
    }

    public string Dump()
    {
        return $"[{string.Join(" | ",Choices.Select(x => x.Dump()))} ({(IsNonTerminalAlternate ? "NT" : "T")})";
    }
}