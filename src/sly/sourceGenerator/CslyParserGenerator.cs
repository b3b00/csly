using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace sly.sourceGenerator;




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

[Generator]
public class CslyParserGenerator : IIncrementalGenerator
{
    
    private const string Namespace = "sly.Generators";
    private const string AttributeName = "ParserGeneratorAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        Dictionary<ClassDeclarationSyntax,(string lexerType, string parserType)> _lexerAndParserTypes = new();
        
        // Filter classes annotated with the [Report] attribute. Only filtered Syntax Nodes can trigger code generation.
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => GetClassDeclarationForSourceGen(ctx))
            .Where(t => t.parserGeneratorAttributeFound)
            .Select((t, _) =>
            {
                _lexerAndParserTypes[t.classDeclarationSyntax] = (t.lexerType, t.parserType);
                return t.classDeclarationSyntax;
            });

        var provider2 = context.SyntaxProvider.CreateSyntaxProvider((s, _) => s is ClassDeclarationSyntax | s is EnumDeclarationSyntax,
            ((ctx,_) =>  ctx.Node ));

        
        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider2.Collect()),
             ((ctx, t) => GenerateCode(ctx, t.Left, t.Right)));
    }

    private string GetParserUsings(ClassDeclarationSyntax classDeclarationSyntax)
    {
        if (classDeclarationSyntax != null)
        {
            var unit = classDeclarationSyntax.GetCompilationUnit();
            StringBuilder builder = new();
            if (unit != null)
            {
                foreach (var @using in unit.Usings)
                {
                    builder.AppendLine(@using.ToString());
                }
            }
            return builder.ToString();
        }
        return "";
    }

    public string GetLexerUsings(EnumDeclarationSyntax enumDeclarationSyntax)
    {
        if (enumDeclarationSyntax != null)
        {
            var unit = enumDeclarationSyntax.GetCompilationUnit();
            StringBuilder builder = new();
            if (unit != null)
            {
                foreach (var @using in unit.Usings)
                {
                    builder.AppendLine(@using.ToString());
                }
            }
            return builder.ToString();
        }
        return "";
    }
    
    private void GenerateCode(SourceProductionContext context, Compilation arg2Left, ImmutableArray<SyntaxNode> declarations)
    {
        Func<SyntaxNode, string> getName = (node) =>
        {
            if (node is ClassDeclarationSyntax classDeclarationSyntax)
            {
                return classDeclarationSyntax.Identifier.ToString();
            }

            if (node is EnumDeclarationSyntax enumDeclarationSyntax)
            {
                return enumDeclarationSyntax.Identifier.ToString();
            }

            return "";
        };

        Dictionary<string, SyntaxNode> declarationsByName = declarations.ToDictionary(x => getName(x));
        
        foreach (var declarationSyntax in declarations)
        {
            if (declarationSyntax is ClassDeclarationSyntax classDeclarationSyntax)
            {

                var className = classDeclarationSyntax.Identifier.Text;
               

                var (lexerType, parserType, outputType, isParserGenerator) = GetClassDeclaration(classDeclarationSyntax);

                if (isParserGenerator)
                {
                    string ns = declarationSyntax.GetNameSpace();
                
                    var lexerDecl = declarationsByName[lexerType] as EnumDeclarationSyntax;
                    var parserDecl = declarationsByName[parserType] as ClassDeclarationSyntax;
                    
                    SyntaxList<UsingDirectiveSyntax> usings = new SyntaxList<UsingDirectiveSyntax>();
                   
                    string lexerName = lexerDecl.Identifier.ToString();
                   
                    string modifiers = string.Join(" ", classDeclarationSyntax.Modifiers.Select(x => x.ToString()));

                    string code = $@"
using System;
using sly.lexer;
using sly.parser;
using sly.buildresult;
using sly.sourceGenerator;
using sly.parser.generator;
using {lexerDecl.GetNameSpace()};
using {parserDecl.GetNameSpace()};

{GetLexerUsings(lexerDecl)}
{GetParserUsings(parserDecl)}

namespace {ns};
{modifiers} class {className} : AbstractParserGenerator<{lexerName}> {{
    
    {(lexerDecl != null ? LexerBuilderGenerator.GenerateLexer(lexerDecl as EnumDeclarationSyntax, outputType) : "")}

    {(parserDecl != null ? ParserBuilderGenerator.GenerateParser(parserDecl as ClassDeclarationSyntax, lexerName, outputType) : "")}

}}";

                    context.AddSource($"{className}.g.cs", SourceText.From(code, Encoding.UTF8));
                }
            }
        }
    }

    private static (ClassDeclarationSyntax classDeclarationSyntax, string lexerType, string parserType, bool parserGeneratorAttributeFound) GetClassDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        Console.WriteLine($"**** {classDeclarationSyntax.GetNameSpace()}.{classDeclarationSyntax.Identifier.ToString()} ****");
        // Go through all attributes of the class.
        foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
        foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
        {
            string name = attributeSyntax.Name.ToString();
            if (name == "ParserGenerator")
            {
                if (attributeSyntax.ArgumentList != null && attributeSyntax.ArgumentList.Arguments.Count == 2)
                {
                    var arg1 = attributeSyntax.ArgumentList.Arguments[0];
                    var arg2 = attributeSyntax.ArgumentList.Arguments[1];
                    if (arg1.Expression is TypeOfExpressionSyntax typeOfLexer &&
                        arg2.Expression is TypeOfExpressionSyntax typeOfParser)
                    {
                        return (classDeclarationSyntax,typeOfLexer.Type.ToString(),typeOfParser.Type.ToString(),true);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        return (classDeclarationSyntax, null, null, false);
    }
    
    private static (string lexerType, string parserType, string outputType, bool parserGeneratorAttributeFound) GetClassDeclaration(
        ClassDeclarationSyntax classDeclarationSyntax)
    {

        // Go through all attributes of the class.
        foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
        foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
        {
            string name = attributeSyntax.Name.ToString();
            if (name == "ParserGenerator")
            {
                if (attributeSyntax.ArgumentList != null && attributeSyntax.ArgumentList.Arguments.Count == 3)
                {
                    var arg1 = attributeSyntax.ArgumentList.Arguments[0];
                    var arg2 = attributeSyntax.ArgumentList.Arguments[1];
                    var arg3 = attributeSyntax.ArgumentList.Arguments[2];
                    if (arg1.Expression is TypeOfExpressionSyntax typeOfLexer &&
                        arg2.Expression is TypeOfExpressionSyntax typeOfParser &&
                        arg3.Expression is TypeOfExpressionSyntax typeOfOutput )
                    {
                        return (typeOfLexer.Type.ToString(),typeOfParser.Type.ToString(), typeOfOutput.Type.ToString(), true);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        return (null, null, null, false);
    }
}