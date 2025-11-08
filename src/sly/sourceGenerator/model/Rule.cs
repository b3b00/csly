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
        _name = head;
    }
    
     
}