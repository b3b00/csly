namespace handExpressions.ebnfparser.model;

public class TerminalClause : IClause
{
    public string Terminal { get; set; }

    
    public bool IsDiscared { get; set; }
    public TerminalClause(string terminal , bool isDiscared = false)
    {
        Terminal = terminal;
        IsDiscared = isDiscared;
    }
}