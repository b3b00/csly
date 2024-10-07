namespace sly.sourceGenerator.generated.ebnfparser.model;

public class ManyClause : IClause
{
    public IClause Clause { get; set; }

    public ManyClause(IClause clause)
    {
        Clause = clause;
    }
}