using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using sly.sourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SharpFileSystem.FileSystems;
using Xunit;

namespace CslyGenerator.Tests;

public class SourceGeneratorTests
{
    private EmbeddedResourceFileSystem _embeddedResourceFileSystem;
    public SourceGeneratorTests()
    {
        _embeddedResourceFileSystem = new(Assembly.GetExecutingAssembly());
    }
    
    private const string aotParserClassTest = $@"
";
    
    private const string extendedLexerTest = $@"


";

    private const string TemplateTest = @"



";

    private ImmutableArray<SyntaxTree> generateSource(string source, string className)
    {
        // Create an instance of the source generator.
        var generator = new CslyParserGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(new[] { generator });

        // To run generators, we can use an empty compilation.
        var compilation = CSharpCompilation.Create(className,
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators. Don't forget to use the new compilation rather than the previous one.
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        return runResult.GeneratedTrees;
    }

    [Theory()]
    [InlineData("/data/template.txt","TemplateParserGenerator")]
    [InlineData("/data/aot.txt","TestGenerator")]
    [InlineData("/data/extended.txt","ExtendedLexerGenerator")]
    [InlineData("/data/expressioncontext.txt", "SimpleExpressionParserWithContextGenerator")]
    [InlineData("/data/callbacks.txt", "ParserCallbacksGenerator")]
    [InlineData("/data/labels.txt", "DuplicateLabelLexerGenerator")]
    [InlineData("/data/expression.txt", "ExpressionParserGenerator")]
    public void TestGenerator(string source, string className)
    {
        var code = _embeddedResourceFileSystem.ReadAllText(source);
        var generatedTrees = generateSource(code, className);

        var contents = generatedTrees.ToDictionary(x => x.FilePath,x => x.ToString());
        var generatedFiles = generatedTrees.Select(x => new FileInfo(x.FilePath).Name);

        Assert.Equivalent(new[]
        {
            $"{className}.g.cs"
        }, generatedFiles);
    }
}