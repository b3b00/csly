using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cslyGenerator;

public class BuilderGenerator
{
    public static string GenerateLexer(EnumDeclarationSyntax enumDeclarationSyntax, string outputType)
    {
        string name = enumDeclarationSyntax.Identifier.ToString();
        StringBuilder builder = new();
        builder.AppendLine($"public IAotLexerBuilder<{name}> GetLexer() {{");

        builder.AppendLine($"var builder = AotLexerBuilder<{name}>.NewBuilder()");
        
        LexerSyntaxWalker walker = new(builder, name);
        walker.Visit(enumDeclarationSyntax);
        builder.AppendLine(";");
        

        builder.AppendLine("return builder;");
        
        builder.AppendLine($"}}");
        return builder.ToString();
    }

    public static string GenerateParser(ClassDeclarationSyntax classDeclarationSyntax)
    {
        return "";
    }
     
}