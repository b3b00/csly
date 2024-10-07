using System.Collections.Generic;
using System.Linq;
using sly.sourceGenerator.generated.ebnfparser;
using sly.sourceGenerator.generated.ebnfparser.model;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sly.sourceGenerator.generated.extractor;




public class ParserConfigurationExtractor
{
    List<string> _tokens = new List<string>();
    
    EbnfParser _parser;
    
    public ParserConfigurationExtractor(List<string> tokens)
    {
        _tokens = tokens;
        _parser = new EbnfParser(tokens);
    }

    public List<Rule> ExtractRules(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var methods = classDeclarationSyntax.Members.Where(x => x is MethodDeclarationSyntax).Cast<MethodDeclarationSyntax>()
            .ToList();
        var rules = methods.Select(x => ExtractRule(x)).Where(x => x != null).ToList();
        return rules;
    }

    public Rule ExtractRule(MethodDeclarationSyntax methodDeclarationSyntax)
    {
        var attributes = methodDeclarationSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() == "Production").ToList();
        if (attributes.Any())
        {
            AttributeSyntax first = attributes.First();
            if (first.ArgumentList.Arguments.Any())
            {
                var argument = first.ArgumentList.Arguments.First();
                if (argument.Expression is LiteralExpressionSyntax expression)
                {
                    var def = expression.ToString();
                    var r = _parser.ParseRule(def.Substring(1,def.Length-2));
                    if (r != null)
                    {
                        r.RuleDefinition = def;
                        r.Method = methodDeclarationSyntax;
                    }

                    return r;
                }
            }    
        }
        return null;
    }
    
}