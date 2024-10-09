namespace sly.sourceGenerator.generated.ebnfparser.model;

public class TerminalClause : IClause
{
    public string Terminal { get; set; }

    public string ExplicitToken { get; set; }
    
    public bool IsDiscared { get; set; }
    public TerminalClause(string terminal , bool isDiscared = false)
    {
        if (terminal.StartsWith("'"))
        {
            ExplicitToken = terminal;
        }
        else
        {
            Terminal = terminal;
        }
        IsDiscared = isDiscared;
    }

    public string Dump()
    {
        string term = !string.IsNullOrWhiteSpace(Terminal) ? Terminal : (!string.IsNullOrWhiteSpace(ExplicitToken) ? ExplicitToken : string.Empty);
        if (IsDiscared)
        {
            return term + " [d]";
        }

        return term;
    }
}