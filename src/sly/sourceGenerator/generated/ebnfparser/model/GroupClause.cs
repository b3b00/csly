using System.Collections.Generic;

namespace sly.sourceGenerator.generated.ebnfparser.model;

public class GroupClause : IClause
{
    public IList<IClause> Clauses { get; set; }
    
    public GroupClause(IList<IClause> clauses)
    {
        Clauses = clauses;
    }
}