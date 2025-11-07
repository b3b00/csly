using System.Collections.Generic;

namespace sly.sourceGenerator.model;

public class Rule
{
    public string Head { get; set; }
    
    public List<IClause> Clauses { get; set; }

    public string Name => _name;
    
    private readonly string _name;
    
    public Rule(string head, List<IClause> clauses)
    {
        this.Head = head;
        this.Clauses = clauses;
        _name = $"{head}_";
        foreach (var clause in clauses)
        {
            if (clause is NonTerminalClause)
            {
                _name += clause.Name;
            }

            if (clause is TerminalClause terminal)
            {
                if (terminal.IsExplicit)
                {
                    // STATIC : convert non letter or digit chars
                    _name += $"_{terminal.Name}";
                }
                else
                {
                    _name += $"_{terminal.Name}";
                }
            }
        }
    }
    
     
}