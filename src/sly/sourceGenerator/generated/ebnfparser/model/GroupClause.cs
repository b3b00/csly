using System.Collections.Generic;
using System.Linq;

namespace sly.sourceGenerator.generated.ebnfparser.model;

public class GroupClause : IClause
{
    public IList<IClause> Clauses { get; set; }
    
    public GroupClause(IList<IClause> clauses)
    {
        Clauses = clauses;
    }

    public string Dump()
    {
        return "("+string.Join(" ",Clauses.Select(x => x.Dump()))+")";
    }
}