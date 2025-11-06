using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.parser.generator;

namespace sly.sourceGenerator;

public class LexerBuilderGenerator
{


    
    public static string GenerateLexer(EnumDeclarationSyntax enumDeclarationSyntax, string outputType,
        Dictionary<string, SyntaxNode> declarationsByName)
    {
        string name = enumDeclarationSyntax.Identifier.ToString();
        StringBuilder builder = new();
        builder.AppendLine($"public IAotLexerBuilder<{name}> GetLexer() {{");

        builder.AppendLine($"var builder = AotLexerBuilder<{name}>.NewBuilder()");
        
        LexerSyntaxWalker walker = new(builder, name, declarationsByName);
        walker.Visit(enumDeclarationSyntax);
        

        builder.AppendLine(".UseLexerPostProcessor(UseTokenPostProcessor())");
        builder.AppendLine(".UseExtensionBuilder(UseTokenExtensions())");
        builder.AppendLine(";");
        builder.AppendLine("return builder;");
        
        builder.AppendLine($"}}");
        return builder.ToString();
    }
}