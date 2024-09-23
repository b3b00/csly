namespace handExpressions.ebnfparser.model;

public class OptionalClause : IClause
{
    public IClause Clause { get; set; }

    public OptionalClause(IClause clause)
    {
        Clause = clause;
    }

}