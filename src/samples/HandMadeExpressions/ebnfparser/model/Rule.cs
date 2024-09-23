using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace handExpressions.ebnfparser.model;

public class Rule : IGrammarNode
{
    public IList<IClause> Clauses { get; set; }
    
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