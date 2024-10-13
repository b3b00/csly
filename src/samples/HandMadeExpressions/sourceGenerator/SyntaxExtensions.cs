using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace handExpressions.sourceGenerator;

public static class SyntaxExtensions
{
    public static string GetNameSpace(this SyntaxNode declarationSyntax)
    {
        if (declarationSyntax.Parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
        {
            return fileScopedNamespace.Name.ToString();
        }
        else if (declarationSyntax.Parent is NamespaceDeclarationSyntax namespaceDeclaration)
        {
            return namespaceDeclaration.Name.ToString();
        }

        return string.Empty;
    }
    
    public static CompilationUnitSyntax GetCompilationUnit(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is CompilationUnitSyntax compilationUnitSyntax)
        {
            return compilationUnitSyntax;
        }

        if (syntaxNode.Parent != null)
        {
            return syntaxNode.Parent.GetCompilationUnit();
        }

        return null;
    }
    
    


}