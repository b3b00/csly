using System.Collections.Generic;
using System.Linq;
using sly.sourceGenerator.generated.ebnfparser;
using sly.sourceGenerator.generated.ebnfparser.model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.parser.generator;

namespace sly.sourceGenerator.generated.extractor;




public class ParserConfigurationExtractor
{
    List<string> _tokens = new List<string>();
    
    EbnfRuleParser _parser;
    
    public ParserConfigurationExtractor(List<string> tokens)
    {
        _tokens = tokens;
        _parser = new EbnfRuleParser(tokens);
    }

    
    #region production rules
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
                    var rule = _parser.ParseRule(def.Substring(1,def.Length-2));
                    if (rule != null)
                    {
                        rule.RuleDefinition = def;
                        rule.Method = methodDeclarationSyntax;
                    }
                    return rule;
                }
            }    
        }
        return null;
    }
    
    #endregion
    
    #region operations
    
    public List<IOperationClause> ExtractOperations(ClassDeclarationSyntax classDeclarationSyntax)
    {
        // TODO
        var methods = classDeclarationSyntax.Members.Where(x => x is MethodDeclarationSyntax).Cast<MethodDeclarationSyntax>()
            .ToList();
        var operations = methods.SelectMany((MethodDeclarationSyntax x) => ExtractAllOperations(x)).Where(y => y != null).ToList();
        return operations;
    }

    private List<IOperationClause> ExtractAllOperations(MethodDeclarationSyntax methodDeclarationSyntax)
    {
        List<IOperationClause> operations = new List<IOperationClause>();
        
        // TODO : look at all operation/operand attributes and dispatch to ExtractXXX
        var operationAttributes = methodDeclarationSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() == "Operation").ToList();
        operations.AddRange(operationAttributes.SelectMany(x => ExtractOperation(methodDeclarationSyntax, x)));
        
        var prefixAttributes = methodDeclarationSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() == "Prefix").ToList();
        operations.AddRange(prefixAttributes.SelectMany(x => ExtractPrefixOperation(methodDeclarationSyntax, x)));
        
        var infixAttributes = methodDeclarationSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() == "Infix").ToList();
        operations.AddRange(infixAttributes.SelectMany(x => ExtractInfixOperation(methodDeclarationSyntax, x)));
        
        var postfixAttributes = methodDeclarationSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() == "Postfix").ToList();
        operations.AddRange(postfixAttributes.SelectMany(x => ExtractPostfixOperation(methodDeclarationSyntax, x)));
        
        var operandAttributes = methodDeclarationSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() == "Operand").ToList();
        operations.AddRange(operandAttributes.SelectMany(x => ExtractOperand(methodDeclarationSyntax, x)));
        
        return new List<IOperationClause>();
    }
    
    private List<IOperationClause> ExtractOperation(MethodDeclarationSyntax methodDeclarationSyntax, AttributeSyntax attributeSyntax)
    {
        return new List<IOperationClause>();
    }

    private List<IOperationClause> ExtractInfixOperation(MethodDeclarationSyntax methodDeclarationSyntax, AttributeSyntax attributeSyntax)
    {
        return new List<IOperationClause>();
    }
    
    private List<IOperationClause> ExtractPrefixOperation(MethodDeclarationSyntax methodDeclarationSyntax, AttributeSyntax attributeSyntax)
    {
        return new List<IOperationClause>();
    }
    
    private List<IOperationClause> ExtractPostfixOperation(MethodDeclarationSyntax methodDeclarationSyntax, AttributeSyntax attributeSyntax)
    {
        return new List<IOperationClause>();
    }
    
    private List<IOperationClause> ExtractOperand(MethodDeclarationSyntax methodDeclarationSyntax, AttributeSyntax attributeSyntax)
    {
        // TODO : must relate to a production rule ? or link through method declaration ??
        return new List<IOperationClause>();
    }
    
    
    #endregion
    
    
}