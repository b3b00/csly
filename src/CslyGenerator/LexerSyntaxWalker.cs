using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cslyGenerator;

public class LexerSyntaxWalker : CSharpSyntaxWalker
{
    StringBuilder _builder = new();

    private string? _lexerName = "";

    public LexerSyntaxWalker(StringBuilder builder, string lexerName)
    {
        _builder = builder;
        _lexerName = lexerName;
    }

    private string GetAttributeArgs(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count > 0)
        {
            var args = attribute.ArgumentList.Arguments.Select(x => x.Expression.ToString()).ToList();
            var strargs = string.Join(", ", args);
            return ", " + strargs;
        }

        return string.Empty;
    }

    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax enumMemberDeclarationSyntax)
    {
        var name = enumMemberDeclarationSyntax.Identifier.ToString();
        if (enumMemberDeclarationSyntax.AttributeLists.Any())
        {
            foreach (AttributeListSyntax attributeListSyntax in enumMemberDeclarationSyntax.AttributeLists)
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                
                string attributeName = attributeSyntax.Name.ToString();
                Console.WriteLine($"visit {attributeName} for {_lexerName}.{name}");
                switch (attributeName)
                {
                    case "Lexeme":
                    {
                        _builder.AppendLine($"builder.Regex({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)});");
                        break;
                    }
                    case "Double":
                    {
                        _builder.AppendLine($"builder.Double({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)});");
                        break;
                    }
                    case "Integer":
                    {
                        _builder.AppendLine($"builder.Integer({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)});");
                        break;
                    }
                    case "Sugar":
                    {
                        _builder.AppendLine($"builder.Sugar({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)});");
                        break;
                    }
                    default:
                    {
                        _builder.AppendLine(
                            $"builder.{attributeName}({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)});");
                        break;
                    }
                }
            }
        }
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax enumDeclarationSyntax)
    {
        var name = enumDeclarationSyntax.Identifier.ToString();
        if (enumDeclarationSyntax.AttributeLists.Any())
        {
            foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (attributeSyntax.Name.ToString() == "Lexer")
                {
                    foreach (var argument in attributeSyntax.ArgumentList.Arguments)
                    {
                        
                        string argumentName = argument.NameEquals.Name.ToString();
                        
                        switch (argumentName)
                        {
                            case "IgnoreWS":
                            {
                                _builder.AppendLine($"builder.IgnoreWhiteSpace({argument.Expression.ToString()});");
                                break;
                            }
                            case "IgnoreEOL":
                            {
                                _builder.AppendLine($"builder.IgnoreEol({argument.Expression.ToString()});");
                                break;
                            }
                            case "WhiteSpace":
                            {
                                _builder.AppendLine($"builder.UseWhiteSpaces({argument.Expression.ToString()});");
                                break;
                            }
                            case "KeyWordIgnoreCase":
                            {
                                _builder.AppendLine($"builder.IgnoreKeywordCase({argument.Expression.ToString()});");
                                break;
                            }
                            case "IndentationAWare":
                            {
                                _builder.AppendLine($"builder.IsIndentationAware({argument.Expression.ToString()});");
                                break;
                            }
                            case "Indentation":
                            {
                                _builder.AppendLine($"builder.UseIndentations({argument.Expression.ToString()});");
                                break;
                            }
                        }
                        
                    }
                }
            }
        }
        foreach (var enumMemberDeclarationSyntax in enumDeclarationSyntax.Members)
        {
            VisitEnumMemberDeclaration(enumMemberDeclarationSyntax);
        }
    }
}