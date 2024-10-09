namespace sly.sourceGenerator.generated.ebnfparser.model;

public class OptionalClause : IClause
{
    public IClause Clause { get; set; }

    public OptionalClause(IClause clause)
    {
        Clause = clause;
    }

    public string Dump()
    {
        return Clause.Dump() + " ?";
    }
}