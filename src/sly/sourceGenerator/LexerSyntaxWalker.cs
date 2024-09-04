using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.lexer;

namespace sly.sourceGenerator;

public class LexerSyntaxWalker : CslySyntaxWalker
{
    StringBuilder _builder = new();

    private string? _lexerName = "";

    public LexerSyntaxWalker(StringBuilder builder, string lexerName)
    {
        _builder = builder;
        _lexerName = lexerName;
    }


    private string GetMethodForIdentifier(MemberAccessExpressionSyntax identifier)
    {
        var name = identifier.Name.Identifier.Text;
        switch (name) 
        {
            case nameof(IdentifierType.AlphaNumericDash) : return "AlphaNumDashId";
            case nameof(IdentifierType.Alpha) : return "AlphaId";
            case nameof(IdentifierType.AlphaNumeric) : return "AlphaNumId";
            case nameof(IdentifierType.Custom) : return "CustomId";
        }

        return null;
    }
    
    private (string methodName, int skip) GetMethodForGenericLexeme(MemberAccessExpressionSyntax member,
        SeparatedSyntaxList<AttributeArgumentSyntax> argumentListArguments)
    {
        var name = member.Name.Identifier.Text;
        switch (name)
        {
            case nameof(GenericToken.KeyWord):return ("Keyword",1); 
            case nameof(GenericToken.SugarToken) : return ("Sugar",1);
            case nameof(GenericToken.Identifier):
            {
                var identierType = argumentListArguments[1].Expression as MemberAccessExpressionSyntax;
                if (identierType != null)
                {
                    var method = GetMethodForIdentifier(identierType);
                    return (method,2);
                }

                return (null, 0);
            }
            case nameof(GenericToken.Int): return ("Integer",1);
            case nameof(GenericToken.Double): return ("Double",1);
            case nameof(GenericToken.Date): return ("Date",1);
            case nameof(GenericToken.Char): return ("Character",1);
            case nameof(GenericToken.Extension): return ("Extension",1);
            case nameof(GenericToken.Hexa): return ("Hexa",1);
            case nameof(GenericToken.String): return ("String",1);
            case nameof(GenericToken.Comment): return ("Comment",1);
            case nameof(GenericToken.UpTo): return ("UpTo",1);
        }
        return (null,0);
    }

    private List<string> GetModes(EnumMemberDeclarationSyntax enumMemberDeclarationSyntax)
    {
        var all = enumMemberDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).ToList();
        var modes = all.Where(x => x.Name.ToString() == "Mode").ToList();
        
        return modes.SelectMany(x =>
        {
            if (x.ArgumentList != null)
            {
                return x.ArgumentList.Arguments.Select(x => x.Expression.ToString()).ToList();
            }
            return new List<string>();
        }).ToList();
    }
    
    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax enumMemberDeclarationSyntax)
    {
        var name = enumMemberDeclarationSyntax.Identifier.ToString();

        var modes = GetModes(enumMemberDeclarationSyntax);
        if (modes.Any())
        {
            ;
        }
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
                        var arg0 = attributeSyntax.ArgumentList.Arguments[0].Expression;
                        if (arg0 is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.StringLiteralExpression )
                        {
                            _builder.AppendLine(
                                $"builder.Regex({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        }
                        else
                        {
                            MemberAccessExpressionSyntax member = arg0 as MemberAccessExpressionSyntax;
                            if (member != null)
                            {
                                var( method, skip) = GetMethodForGenericLexeme(member, attributeSyntax.ArgumentList.Arguments);
                                if (!string.IsNullOrEmpty(method))
                                {
                                    _builder.AppendLine(
                                        $"builder.{method}({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes,skip)});");
                                }
                            }
                        }

                        break;
                    }
                    case "Double":
                    {
                        _builder.AppendLine($"builder.Double({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "Integer":
                    {
                        _builder.AppendLine($"builder.Integer({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "Sugar":
                    {
                        _builder.AppendLine($"builder.Sugar({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "AlphaId":
                    {
                        _builder.AppendLine($"builder.AlphaId({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "AlphaNumId":
                    {
                        _builder.AppendLine($"builder.AlphaNumId({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "AlphaNumDashId":
                    {
                        _builder.AppendLine(
                            $"builder.AlphaNumDashId({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "MultiLineComment":
                    {
                        _builder.AppendLine(
                            $"builder.MultiLineComment({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "SingleLineComment":
                    {
                        _builder.AppendLine(
                            $"builder.SingleLineComment({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "Extension":
                    {
                        _builder.AppendLine(
                            $"builder.Extension({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "String":
                    {
                        _builder.AppendLine(
                            $"builder.String({_lexerName}.{name} {GetAttributeArgs(attributeSyntax, modes)});");
                        break;
                    }
                    case "UpTo":
                    {
                        _builder.AppendLine(
                            $"builder.UpTo({_lexerName}.{name} {GetAttributeArgs(attributeSyntax, modes)});");
                        break;
                    }
                    case "Push":
                    {
                        _builder.AppendLine(
                            $"builder.Push({_lexerName}.{name} {GetAttributeArgs(attributeSyntax,modes)});");
                        break;
                    }
                    case "Pop":
                    {
                        _builder.AppendLine(
                            $"builder.Pop({_lexerName}.{name});");
                        break;
                    }
                    default:
                    {
                        // _builder.AppendLine(
                        //     $"builder.{attributeName}({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)});");
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