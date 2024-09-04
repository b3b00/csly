using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sly.sourceGenerator;

public class ParserBuilderGenerator
{
    public static string GenerateParser(ClassDeclarationSyntax classDeclarationSyntax,string lexerName, string outputType)
    {
        string name = classDeclarationSyntax.Identifier.ToString();
        StringBuilder builder = new();
        // TODO : get starting rule if exists
        var rootRule = GetRootRule(classDeclarationSyntax);
        builder.AppendLine($"public AotEBNFParserBuilder<{lexerName},{outputType}> GetParser({(rootRule == null ? "string rootRule": "")}) {{");
        ParserSyntaxWalker walker = new(builder, name,lexerName, outputType);
        if (rootRule != null)
        {
            builder.AppendLine("string rootRule = \"root\";");
        }
        builder.AppendLine($"{name} instance = new {name}();");
        builder.AppendLine($"var builder = AotEBNFParserBuilder<{lexerName}, {outputType}>");
        builder.AppendLine($@".NewBuilder(instance, rootRule, ""en"");");
        walker.Visit(classDeclarationSyntax);
        builder.AppendLine("return null;");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string GetRootRule(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var rootAttribute = classDeclarationSyntax.AttributeLists.ToList().SelectMany(x => x.Attributes).ToList()
            .FirstOrDefault(x => x.Name.ToString() == "ParserRoot");
        if (rootAttribute == null)
        {
            return null;
        }
        var root = rootAttribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression?.ToString();
        return root;
    }
}