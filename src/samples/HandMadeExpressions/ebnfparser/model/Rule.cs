using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace handExpressions.ebnfparser.model;

public class Rule : IGrammarNode
{
    public IList<IClause> Clauses { get; set; }
    
    public MethodDeclarationSyntax Method { get; set; }

    public string RuleDefinition { get; set; }
    
    public Rule(string rule, MethodDeclarationSyntax method, IList<IClause> clauses)
    {
        Clauses = clauses;
        RuleDefinition = rule;
        Method = method;
    }
}