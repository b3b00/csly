using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sly.sourceGenerator.generated.ebnfparser.model;

public class Rule : IGrammarNode
{
    public IList<IClause> Clauses { get; set; }

    public int Number { get; set; } = -1;
    
    public MethodDeclarationSyntax Method { get; set; }

    public string RuleDefinition { get; set; }
    
    public string NonTerminalName { get; set; }
    
    public Rule(string nonTerminalName, MethodDeclarationSyntax method, IList<IClause> clauses)
    {
        Clauses = clauses;
        NonTerminalName = nonTerminalName;
        Method = method;
    }
    
    public Rule(string nonTerminalName, IList<IClause> clauses)
        {
            Clauses = clauses;
            NonTerminalName = nonTerminalName;
            Method = null;
        }
}