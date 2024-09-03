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
            return ", "+strargs;
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
                switch (attributeName)
                {
                    case "Lexeme":
                    {
                        _builder.AppendLine($".Regex({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)})");
                        break;
                    }
                    case "Double":
                    {
                        _builder.AppendLine($".Double({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)})");
                        break;
                    }
                    case "Integer":
                    {
                        _builder.AppendLine($".Integer({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)})");
                        break;
                    }
                    case "Sugar":
                    {
                        _builder.AppendLine($".Sugar({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)})");
                        break;
                    }
                    default:
                    {
                        _builder.AppendLine(
                            $".{attributeName}({_lexerName}.{name} {GetAttributeArgs(attributeSyntax)})");
                        break;
                    }
                }
            }
        }
    }
}