namespace sly.sourceGenerator.generated.ebnfparser.model;

public class NonTerminalClause : IClause
{
    public string NonTerminal { get; set; }

    public NonTerminalClause(string nonTerminal)
    {
        NonTerminal = nonTerminal;
    }

    public string Dump()
    {
        return NonTerminal;
    }
}