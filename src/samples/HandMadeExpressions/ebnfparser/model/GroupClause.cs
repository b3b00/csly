namespace handExpressions.ebnfparser.model;

public class GroupClause : IClause
{
    public IList<IClause> Clauses { get; set; }
    
    public GroupClause(IList<IClause> clauses)
    {
        Clauses = clauses;
    }
}