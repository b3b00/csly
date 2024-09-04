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

    private string? _parserName = "";
    private readonly string _lexerName;
    private readonly string _outputType;

    public ParserSyntaxWalker(StringBuilder builder, string parserName, string lexerName, string outputType)
    {
        _builder = builder;
        _parserName = parserName;
        _lexerName = lexerName;
        _outputType = outputType;
    }


    public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclarationSyntax)
    {
        string name = classDeclarationSyntax.Identifier.ToString();
        var attributes = classDeclarationSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() != nameof(ParserRootAttribute))
            .ToList();
        foreach (var attribute in attributes)
        {
            VisitAttribute(attribute);
        }

        var methods = classDeclarationSyntax.Members
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
        // TODO optimizations
        Console.WriteLine("coucou");
    }

    private bool IsOperand(MethodDeclarationSyntax method)
    {
        return method.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.Name.ToString() == "Operand");
    }
    
    public override void VisitMethodDeclaration(MethodDeclarationSyntax methodDeclarationSyntax)
    {
        string methodName = methodDeclarationSyntax.Identifier.ToString();
        var attributes = methodDeclarationSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .Where(x => x.Name.ToString() != "Operand").ToList();
        foreach (var attribute in attributes)
        {
            switch (attribute.Name.ToString())
            {
                case "Production":
                {
                    var rule = GetAttributeArgs(attribute,withLeadingComma:false);
                    if (IsOperand(methodDeclarationSyntax))
                    {
                        _builder.AppendLine($"builder.Operand({rule},null);");
                    }
                    else
                    {
                        _builder.AppendLine($"builder.Production({rule},null);");
                    }

                    break;
                }
                case "Operation":
                {
                    if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
                    {
                        var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
                        _builder.AppendLine(
                            $"builder.Operation({realArg} {GetAttributeArgs(attribute, skip:1)},null);");
                    }
                    else
                    {
                        _builder.AppendLine(
                            $"builder.Operation({GetAttributeArgs(attribute, withLeadingComma: false)},null);");
                    }

                    break;
                }
                case "Prefix":
                {
                    var precedence = attribute.ArgumentList.Arguments[2].Expression.ToString();
                    if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
                    {
                        var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
                        _builder.AppendLine(
                            $"builder.Prefix({realArg}, {precedence}, null);");
                    }
                    else
                    {
                        _builder.AppendLine(
                            $"builder.Prefix({attribute.ArgumentList.Arguments[0].Expression.ToString()}, {precedence},null);");
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
                            $"builder.Prefix({realArg}, {precedence}, null);");
                    }
                    else
                    {
                        _builder.AppendLine(
                            $"builder.Postfix({attribute.ArgumentList.Arguments[0].Expression.ToString()}, {precedence},null);");
                    }

                    break;
                }
                case "Infix":
                {
                    if (attribute.ArgumentList.Arguments[0].Expression is CastExpressionSyntax cast)
                    {
                        var realArg = (attribute.ArgumentList.Arguments[0].Expression as CastExpressionSyntax).Expression.ToString();
                        _builder.AppendLine(
                            $"builder.Infix({realArg} {GetAttributeArgs(attribute, skip:1)},null);");
                    }
                    _builder.AppendLine($"builder.Infix({GetAttributeArgs(attribute,withLeadingComma:false)},null);");
                    break;
                }
                default:
                {
                    _builder.AppendLine($" // unknown attribute {attribute.Name.ToString()}");
                    break;
                }
            }
        }


    }

    private void AddProductionVisitor(MethodDeclarationSyntax method)
    {
        string methodName = method.Identifier.ToString();
        _builder.AppendLine("(args) => {");
        _builder.AppendLine($"var result = instance.{methodName}();");
        _builder.Append("}");
    }
    
}