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

namespace sly.sourceGenerator;

public class ParserSyntaxWalker : CslySyntaxWalker
{
    StringBuilder _builder = new();

    public ParserSyntaxWalker(StringBuilder builder, string parserName, string lexerName, string outputType)
    {
        _builder = builder;
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

        _builder.AppendLine("builder.WithLexerbuilder(GetLexer())");
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
                _builder.AppendLine($"builder.UseAutoCloseIndentations({GetAttributeArgs(node,withLeadingComma:false)});");
                break;
            }
            case "UseMemoization":
            {
                _builder.AppendLine($"builder.UseMemoization({GetAttributeArgs(node,withLeadingComma:false)});");
                break;
            }
            case "BroadenTokenWindow":
            {
                _builder.AppendLine($"builder.UseBroadenTokenWindow({GetAttributeArgs(node,withLeadingComma:false)});");
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

        if (!string.IsNullOrEmpty(nodeName))
        {
            ;
        }
        
        foreach (var attribute in attributes)
        {
            switch (attribute.Name.ToString())
            {
                case "Production":
                {
                    var rule = GetAttributeArgs(attribute,withLeadingComma:false);
                    if (IsOperand(node))
                    {
                        _builder.AppendLine($".Operand({rule},");
                        AddProductionVisitor(node);
                        _builder.AppendLine(")");
                    }
                    else
                    {
                        _builder.AppendLine($".Production({rule},");
                        AddProductionVisitor(node);
                        _builder.AppendLine(")");
                    }
                    break;
                }
                case "Operation":
                {
                    if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
                    {
                        var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
                        _builder.AppendLine(
                            $".Operation({realArg} {GetAttributeArgs(attribute, skip:1)},");
                        AddProductionVisitor(node);
                        _builder.AppendLine(")");
                    }
                    else
                    {
                        _builder.AppendLine(
                            $".Operation({GetAttributeArgs(attribute, withLeadingComma: false)},");
                        AddProductionVisitor(node);
                        _builder.AppendLine(")");
                    }

                    break;
                }
                case "Prefix":
                {
                    var precedence = attribute.ArgumentList.Arguments[2].Expression.ToString();
                    if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
                    {
                        var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
                        _builder.AppendLine($".Prefix({realArg}, {precedence}, ");
                        AddProductionVisitor(node);
                        _builder.AppendLine(")");
                    }
                    else
                    {
                        _builder.AppendLine(
                            $".Prefix({attribute.ArgumentList.Arguments[0].Expression.ToString()}, {precedence},");
                        AddProductionVisitor(node);
                        _builder.AppendLine(")");
                    }

                    break;
                }
                case "Postfix":
                {
                    var precedence = attribute.ArgumentList.Arguments[2].Expression.ToString();
                    
                    if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
                    {
                        var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
                        _builder.AppendLine(
                            $".Postfix({realArg}, {precedence},");
                        AddProductionVisitor(node);
                        _builder.AppendLine(")");
                    }
                    else
                    {
                        _builder.AppendLine(
                            $".Postfix({attribute.ArgumentList.Arguments[0].Expression.ToString()}, {precedence},");
                        AddProductionVisitor(node);
                        _builder.AppendLine(")");
                    }

                    break;
                }
                case "Infix":
                {
                    if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
                    {
                        var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
                        _builder.AppendLine(
                            $".Infix({realArg} {GetAttributeArgs(attribute, skip:1)},null)");
                    }

                    _builder.AppendLine($".Infix({GetAttributeArgs(attribute, withLeadingComma: false)},");
                    AddProductionVisitor(node);
                    _builder.AppendLine(")");
                    break;
                }
                default:
                {
                    break;
                }
            }

            if (!string.IsNullOrEmpty(nodeName))
            {
                _builder.AppendLine($"    .Named({nodeName})");
            }
        }


    }

    private void AddProductionVisitor(MethodDeclarationSyntax method)
    {
        var parameters = method.ParameterList.Parameters.ToList();
        
        string methodName = method.Identifier.ToString();
        _builder.AppendLine("(object[] args) => {");
        _builder.Append($"var result = instance.{methodName}(");
        for (int i = 0; i < parameters.Count; i++)
        {
            if (i > 0)
            {
                _builder.Append(", ");
            }
            var type = parameters[i].Type.ToString();
            _builder.Append($"({type})args[{i}]");
        }
        _builder.AppendLine(");");
        _builder.AppendLine("return result;");
        _builder.Append("}");
    }
    
}