using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using sly.lexer;
using sly.parser.generator;
using sly.sourceGenerator.model;

namespace sly.sourceGenerator;

public class ParserSyntaxWalker : CslySyntaxWalker
{
    StringBuilder Builder = new();
    
    StaticParserBuilder _staticParserBuilder;

    public ParserSyntaxWalker(StringBuilder builder, string parserName, string lexerName, string outputType, StaticParserBuilder staticParserBuilder)
    {
        Builder = builder;
        _staticParserBuilder = staticParserBuilder;
    }


    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        string name = node.Identifier.ToString();
        var attributes = node.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() != nameof(ParserRootAttribute))
            .ToList();
        foreach (var attribute in attributes)
        {
            VisitAttribute(attribute);
        }

        Builder.AppendLine("builder.WithLexerbuilder(GetLexer())");
        var methods = node.Members
            .ToList()
            .Where(x => x is MethodDeclarationSyntax)
            .Cast<MethodDeclarationSyntax>().ToList();
        foreach (var method in methods)
        {
            VisitMethodDeclaration(method);
        }
    }

    public override void VisitAttribute(AttributeSyntax node)
    {
        var name = node.Name.ToString();
        switch (name)
        {
            case "AutoCloseIndentations":
            {
                Builder.AppendLine($"builder.UseAutoCloseIndentations({GetAttributeArgs(node,withLeadingComma:false)});");
                break;
            }
            case "UseMemoization":
            {
                Builder.AppendLine($"builder.UseMemoization({GetAttributeArgs(node,withLeadingComma:false)});");
                break;
            }
            case "BroadenTokenWindow":
            {
                Builder.AppendLine($"builder.UseBroadenTokenWindow({GetAttributeArgs(node,withLeadingComma:false)});");
                break;
            }
        }
    }

    private bool IsOperand(MethodDeclarationSyntax method)
    {
        return method.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.Name.ToString() == "Operand");
    }
    
    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        string methodName = node.Identifier.ToString();
        var attributes = node.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() != "Operand" && x.Name.ToString() != "NodeName").ToList();
        
        var nodeNames = node.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() == "NodeName")
            .ToList();
        var nodeName = nodeNames.Any() ? nodeNames[0].ArgumentList.Arguments[0].Expression.ToString() : null;

        foreach (var attribute in attributes)
        {
            if (attribute.Name.ToString() == "Production")
            {
                if (IsOperand(node))
                {
                    var rule = GetAttributeArgs(attribute, withLeadingComma: false);
                    Builder.AppendLine($".Operand({rule},");
                    AddProductionVisitor(node);
                    Builder.AppendLine(")");
                }
                else
                {
                    var ruleString = GetAttributeArgs(attribute, withLeadingComma: false);
                    
                    // STATIC : parse rule 
                    //var rule = _staticParserBuilder.Parse(ruleString);
                    
                    Builder.AppendLine($".Production({ruleString},");
                    AddProductionVisitor(node);
                    Builder.AppendLine(")");
                }
            }
            else if (attribute.Name.ToString() == "Operation")
            {
                AddOperation(node, attribute);
            }
            else if (attribute.Name.ToString() is "Prefix" or "Postfix")
            {
                AddPrePostFix(node, attribute);
            }
            else if (attribute.Name.ToString() == "Infix")
            {
                AddInfix(node, attribute);
            }
            
            if (!string.IsNullOrEmpty(nodeName))
            {
                Builder.AppendLine($"    .Named({nodeName})");
            }
        }
       


    }

    private void AddInfix(MethodDeclarationSyntax node, AttributeSyntax attribute)
    {
        if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
        {
            var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
            Builder.AppendLine(
                $".Infix({realArg} {GetAttributeArgs(attribute, skip:1)},null)");
        }

        Builder.AppendLine($".Infix({GetAttributeArgs(attribute, withLeadingComma: false)},");
        AddProductionVisitor(node);
        Builder.AppendLine(")");
    }

    private void AddPrePostFix(MethodDeclarationSyntax node, AttributeSyntax attribute)
    {
        var precedence = attribute.ArgumentList.Arguments[2].Expression.ToString();
        if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
        {
            var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
            Builder.AppendLine($".{attribute.Name.ToString()}({realArg}, {precedence}, ");
            AddProductionVisitor(node);
            Builder.AppendLine(")");
        }
        else
        {
            Builder.AppendLine(
                $".{attribute.Name.ToString()}({attribute.ArgumentList.Arguments[0].Expression.ToString()}, {precedence},");
            AddProductionVisitor(node);
            Builder.AppendLine(")");
        }
    }

    private void AddOperation(MethodDeclarationSyntax node, AttributeSyntax attribute)
    {
        if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
        {
            var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
            Builder.AppendLine(
                $".Operation({realArg} {GetAttributeArgs(attribute, skip:1)},");
            AddProductionVisitor(node);
            Builder.AppendLine(")");
        }
        else
        {
            Builder.AppendLine(
                $".Operation({GetAttributeArgs(attribute, withLeadingComma: false)},");
            AddProductionVisitor(node);
            Builder.AppendLine(")");
        }
    }

    private void AddProductionVisitor(MethodDeclarationSyntax method)
    {
        var parameters = method.ParameterList.Parameters.ToList();
        
        string methodName = method.Identifier.ToString();
        Builder.AppendLine("(object[] args) => {");
        Builder.Append($"var result = instance.{methodName}(");
        for (int i = 0; i < parameters.Count; i++)
        {
            if (i > 0)
            {
                Builder.Append(", ");
            }
            var type = parameters[i].Type.ToString();
            Builder.Append($"({type})args[{i}]");
        }
        Builder.AppendLine(");");
        Builder.AppendLine("return result;");
        Builder.Append("}");
    }
    
    
    
    
    
}