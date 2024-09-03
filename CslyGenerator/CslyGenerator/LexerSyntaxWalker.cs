using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cslyGenerator;

public class LexerSyntaxWalker : CSharpSyntaxWalker
{
    StringBuilder _builder = new();
    
    public LexerSyntaxWalker(StringBuilder builder)
    {
        _builder = builder;
    }

    private string GetAttributeArgs(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count > 0)
        {
            var args = attribute.ArgumentList.Arguments.Select(x => x.Expression.ToString()).ToList();
            var strargs = string.Join(", ", args);
            return strargs;
        }

        return string.Empty;
    }
    
    public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax enumMemberDeclarationSyntax)
    {
        var name = enumMemberDeclarationSyntax.Identifier.ToString(); 
        
        foreach (AttributeListSyntax attributeListSyntax in enumMemberDeclarationSyntax.AttributeLists)
        foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
        {
            Console.WriteLine($"{name}: {attributeSyntax.ToString()}");
            string attributeName = attributeSyntax.Name.ToString();
            switch (attributeName)
            {
                case "Double":
                {
                    _builder.AppendLine($".Double({GetAttributeArgs(attributeSyntax)})");
                    break;
                }
                case "Integer":
                {
                    _builder.AppendLine($".Integer({GetAttributeArgs(attributeSyntax)})");
                    break;
                }
                case "Sugar":
                {
                    _builder.AppendLine($".Sugar({GetAttributeArgs(attributeSyntax)})");
                    break;
                }
                default:
                {
                    _builder.AppendLine($".{attributeName}({GetAttributeArgs(attributeSyntax)})");
                    break;
                }
            }
        }
    }
}