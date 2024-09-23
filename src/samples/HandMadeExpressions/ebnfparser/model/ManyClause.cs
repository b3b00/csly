namespace handExpressions.ebnfparser.model;

public class ManyClause : IClause
{
    public IClause Clause { get; set; }

    public ManyClause(IClause clause)
    {
        Clause = clause;
    }
}