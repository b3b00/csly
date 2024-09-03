using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cslyGenerator;

public class BuilderGenerator
{
    public static string GenerateLexer(EnumDeclarationSyntax enumDeclarationSyntax)
    {
        string name = enumDeclarationSyntax.Identifier.ToString();
        StringBuilder builder = new();
        builder.AppendLine($"public BuildResult<Parser<{name}, double>> FluentInitializeCenericLexer() {{");
        return builder.ToString();
    }

    public static string GenerateParser(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return "";
    }
     
}