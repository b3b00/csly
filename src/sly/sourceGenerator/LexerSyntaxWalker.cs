using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.lexer;

namespace sly.sourceGenerator;

public class LexerSyntaxWalker : CslySyntaxWalker
{
    StringBuilder _builder = new();

    private string _lexerName = "";
    private readonly Dictionary<string, SyntaxNode> _declarationsByName;

    public LexerSyntaxWalker(StringBuilder builder, string lexerName, Dictionary<string, SyntaxNode> declarationsByName)
    {
        _builder = builder;
        _lexerName = lexerName;
        _declarationsByName = declarationsByName;
    }

    protected string GetChannelArg(AttributeSyntax attribute, int skip = 0)
    {
        if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count > skip)
        {
            var arguments = attribute.ArgumentList.Arguments.Skip(skip).ToList();
            var firstArg = arguments[0];
            string firstArgColonName = firstArg?.NameColon?.Name?.ToString();
            var firstArgAsLiteral = firstArg?.Expression as LiteralExpressionSyntax;
            if (firstArg != null && (firstArgColonName == "channel" || (firstArgAsLiteral == null ||
                                                   firstArgAsLiteral.Kind() != SyntaxKind.StringLiteralExpression)))
            {
                return firstArg.Expression.ToString();
            }
        }

        return null;
    } 
    
    protected string GetAttributeArgsForLexemekeyWord(AttributeSyntax attribute, int skip = 0, bool withLeadingComma = true)
    {
         
        if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count > 0)
        {
            var arguments = attribute.ArgumentList.Arguments.Skip(skip).ToList();
            
            var firstArg = arguments[0];
            bool isFirstArgChannel = false;
            string firstArgColonName = firstArg?.NameColon?.Name?.ToString();
            string channel = null;
            string tokens = "";
            var firstArgAsLiteral = firstArg?.Expression as LiteralExpressionSyntax;
            if (firstArg != null && (firstArgColonName == "channel" || (firstArgAsLiteral == null || firstArgAsLiteral.Kind() != SyntaxKind.StringLiteralExpression)))
            {
                isFirstArgChannel = true;
                channel = firstArg.Expression.ToString();
            }
            var tokenArgs = arguments.Skip(isFirstArgChannel ? 1 : 0).ToList();
            if (tokenArgs.Count == 1)
            {
                tokens = tokenArgs.ElementAt(0).Expression.ToString();
            }

            if (tokenArgs.Count > 1)
            {
                tokens = "new [] { "+string.Join(",", tokenArgs.Select(t => t.ToString()).ToList())+ " }";
            }

            string args = $", {tokens} ";
            if (isFirstArgChannel)
            {
                args += $",{channel} ";
            }
            return args;
        }
        return string.Empty;
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
                if (argumentListArguments.Count > 1)
                {
                    var identierType = argumentListArguments[1].Expression as MemberAccessExpressionSyntax;
                    if (identierType != null)
                    {
                        var method = GetMethodForIdentifier(identierType);
                        return (method, 2);
                    }
                }

                return ("AlphaId", 1);
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
    
    private List<(string lang, string label)> GetLabels(EnumMemberDeclarationSyntax enumMemberDeclarationSyntax)
    {
        var all = enumMemberDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).ToList();
        var labels = all.Where(x => x.Name.ToString() == "LexemeLabel").ToList();
        
         return labels
             .Where(x => x.ArgumentList != null && x.ArgumentList.Arguments.Count == 2)
             .Select(x =>
        {
                return (x.ArgumentList.Arguments[0].ToString(),x.ArgumentList.Arguments[1].ToString());
        }).ToList();
    }

    private static string[] ShortLexemes = new[]
    {
        "Double", "Int", "Integer", "Sugar", "AlphaId", "AlphaNumId", "AlphaNumDashId",
        "MultiLineComment", "SingleLineComment", "Extension", "String", "Keyword", "KeyWord", "UpTo"
    };
    
    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
    {
        var name = node.Identifier.ToString();

        var modes = GetModes(node);
        var labels = GetLabels(node);
        
        if (node.AttributeLists.Any())
        {
            foreach (AttributeListSyntax attributeListSyntax in node.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes.Where(x => x.Name.ToString() != "mode" && x.Name.ToString() != "LexemeLabel" ))
                {

                    string attributeName = attributeSyntax.Name.ToString();

                    if (ShortLexemes.Contains(attributeName))
                    {
                        _builder.AppendLine($@".{attributeName}({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)})");
                        var channel = GetChannelArg(attributeSyntax);
                        AddChannel(attributeSyntax);
                    }
                    else if (attributeName == "Lexeme")
                    {
                        VisitLexemeAttribute(attributeSyntax, name);
                    }
                    else if (attributeName == "Push")
                    {
                        _builder.AppendLine(
                            $".PushToMode({GetAttributeArgs(attributeSyntax, withLeadingComma: false)})");
                    }
                    else if (attributeName == "Pop")
                    {
                        _builder.AppendLine(
                            $".PopMode()");
                    }

                    AddLabels(labels);

                    AddModes(modes);
                }
            }
        }
    }

    private void AddModes(List<string> modes)
    {
        if (modes.Any())
        {
            _builder.AppendLine($"        .WithModes({string.Join(", ", modes)})");
        }
    }

    private void AddLabels(List<(string lang, string label)> labels)
    {
        if (labels.Any())
        {
            foreach (var label in labels)
            {
                _builder.AppendLine($"        .WithLabel({label.lang}, {label.label})");
            }
        }
    }

    private void AddChannel(AttributeSyntax attributeSyntax)
    {
        var channel = GetChannelArg(attributeSyntax);
        if (channel != null)
        {
            _builder.AppendLine($"     .OnChannel({channel})");
        }
    }

    private void VisitLexemeAttribute(AttributeSyntax attributeSyntax, string name)
    {
        var arg0 = attributeSyntax.ArgumentList.Arguments[0].Expression;
        if (arg0 is LiteralExpressionSyntax literal &&
            literal.Kind() == SyntaxKind.StringLiteralExpression)
        {
            _builder.AppendLine(
                $@".Regex({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)})");
        }
        else
        {
            MemberAccessExpressionSyntax member = arg0 as MemberAccessExpressionSyntax;
            if (member != null)
            {
                var (method, skip) = GetMethodForGenericLexeme(member,
                    attributeSyntax.ArgumentList.Arguments);
                if (!string.IsNullOrEmpty(method))
                {
                    if (method == "Keyword" || method == "Sugar")
                    {
                        _builder.AppendLine(
                            $@".{method}({_lexerName}.{name} {GetAttributeArgsForLexemekeyWord(attributeSyntax, skip)})");
                        var channel = GetChannelArg(attributeSyntax, skip);
                        if (channel != null)
                        {
                            _builder.AppendLine($"     .OnChannel({channel})");
                        }
                    }
                    else
                    {
                        _builder.AppendLine(
                            $@".{method}({_lexerName}.{name} {GetAttributeArgs(attributeSyntax, skip)})");
                        var channel = GetChannelArg(attributeSyntax, skip);
                        if (channel != null)
                        {
                            _builder.AppendLine($"     .OnChannel({channel})");
                        }
                    }
                }
            }
        }
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        var name = node.Identifier.ToString();
        if (node.AttributeLists.Any())
        {
            foreach (AttributeListSyntax attributeListSyntax in node.AttributeLists)
            {
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
                                    _builder.AppendLine($".IgnoreWhiteSpace({argument.Expression.ToString()})");
                                    break;
                                }
                                case "IgnoreEOL":
                                {
                                    _builder.AppendLine($".IgnoreEol({argument.Expression.ToString()})");
                                    break;
                                }
                                case "WhiteSpace":
                                {
                                    _builder.AppendLine($".UseWhiteSpaces({argument.Expression.ToString()})");
                                    break;
                                }
                                case "KeyWordIgnoreCase":
                                {
                                    _builder.AppendLine($".IgnoreKeywordCase({argument.Expression.ToString()})");
                                    break;
                                }
                                case "IndentationAWare":
                                {
                                    _builder.AppendLine($".IsIndentationAware({argument.Expression.ToString()})");
                                    break;
                                }
                                case "Indentation":
                                {
                                    _builder.AppendLine($".UseIndentations({argument.Expression.ToString()})");
                                    break;
                                }
                            }

                        }
                    }

                    if (attributeSyntax.Name.ToString() == "CallBacks")
                    {
                        var typeOfExpressionSyntax =
                            attributeSyntax.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax;
                        if (typeOfExpressionSyntax != null)
                        {
                            string typeName = typeOfExpressionSyntax.Type.ToString();
                            if (_declarationsByName.TryGetValue(typeName, out var declaration))
                            {
                                if (declaration is ClassDeclarationSyntax classDeclarationSyntax)
                                {
                                    var methods = classDeclarationSyntax.Members
                                        .ToList()
                                        .Where(x => x is MethodDeclarationSyntax)
                                        .Cast<MethodDeclarationSyntax>().ToList();
                                    foreach (var method in methods)
                                    {
                                        if (method.AttributeLists.Any())

                                        {
                                            var callbackAttribute = method.AttributeLists.SelectMany(x => x.Attributes)
                                                .FirstOrDefault(x => x.Name.ToString() == "TokenCallback");
                                            if (callbackAttribute != null)
                                            {
                                                var tokenid =
                                                    (callbackAttribute.ArgumentList.Arguments[0].Expression as
                                                        CastExpressionSyntax).Expression.ToString();
                                                _builder.AppendLine(
                                                    $".WithCallback({tokenid},( token => {typeName}.{method.Identifier.ToString()}(token)))");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        foreach (var enumMemberDeclarationSyntax in node.Members)
        {
            VisitEnumMemberDeclaration(enumMemberDeclarationSyntax);
        }
    }
}