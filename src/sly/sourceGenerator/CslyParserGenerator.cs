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
        var unit = (classDeclarationSyntax.Parent.Parent as CompilationUnitSyntax);
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

    public string GetLexerUsings(EnumDeclarationSyntax enumDeclarationSyntax)
    {
        var unit = (enumDeclarationSyntax.Parent.Parent as CompilationUnitSyntax);
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
                    Console.WriteLine("###############################################");
                    Console.WriteLine("###");
                    Console.WriteLine($"### SOURCE GENERATION FOR >>{className}<<");
                    Console.WriteLine("####");
                    Console.WriteLine("###############################################");

                    string ns = declarationSyntax.GetNameSpace();
                
                    var lexerDecl = declarationsByName[lexerType] as EnumDeclarationSyntax;
                    var parserDecl = declarationsByName[parserType] as ClassDeclarationSyntax;
                    string parserName = parserDecl.Identifier.ToString();
                    SyntaxList<UsingDirectiveSyntax> usings = new SyntaxList<UsingDirectiveSyntax>();
                    if (parserDecl.Parent is CompilationUnitSyntax unit)
                    {
                        usings = (parserDecl.Parent as CompilationUnitSyntax).Usings;
                    }
                    else
                    {
                        if (parserDecl.Parent.Parent is CompilationUnitSyntax)
                        {
                            usings = (parserDecl.Parent.Parent as CompilationUnitSyntax).Usings;
                        }
                    }
                    string lexerName = lexerDecl.Identifier.ToString();
                    // TODO public class  ? get visibility from classDeclaration ??

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
    
    {LexerBuilderGenerator.GenerateLexer(lexerDecl as EnumDeclarationSyntax, outputType)}

    {ParserBuilderGenerator.GenerateParser(parserDecl as ClassDeclarationSyntax, lexerName, outputType)}

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