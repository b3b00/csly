using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using sly.sourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SharpFileSystem.FileSystems;
using Xunit;

namespace ParserTests.sourceGenerators;

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

    private GeneratorDriverRunResult generateSource(string source, string className)
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

        return runResult;
    }

    [Theory()]
    [InlineData("/sourceGenerators/data/template.txt","TemplateParserGenerator")]
    [InlineData("/sourceGenerators/data/aot.txt","TestGenerator")]
    [InlineData("/sourceGenerators/data/extended.txt","ExtendedLexerGenerator")]
    [InlineData("/sourceGenerators/data/expressioncontext.txt", "SimpleExpressionParserWithContextGenerator")]
    [InlineData("/sourceGenerators/data/callbacks.txt", "ParserCallbacksGenerator")]
    [InlineData("/sourceGenerators/data/labels.txt", "DuplicateLabelLexerGenerator")]
    [InlineData("/sourceGenerators/data/expression.txt", "ExpressionParserGenerator")]
    [InlineData("/sourceGenerators/data/explicits.txt", "ExplicitTokensExpressionParserGenerator")]
    [InlineData("/sourceGenerators/data/variableexpression.txt", "VariableExpressionParserGenerator")]
    [InlineData("/sourceGenerators/data/simple.txt", "SimpleParserGenerator")]
    public void TestGenerator(string source, string className)
    {
        var code = _embeddedResourceFileSystem.ReadAllText(source);
        var result = generateSource(code, className);
        Assert.True(result.Diagnostics.IsEmpty);
        
        var contents = result.GeneratedTrees.ToDictionary(x => x.FilePath,x => x.ToString());
        var generatedFiles = result.GeneratedTrees.Select(x => new FileInfo(x.FilePath).Name);

        Assert.Equivalent(new[]
        {
            $"{className}.g.cs"
        }, generatedFiles);
    }
    
    [Theory]
    [InlineData("/sourceGenerators/data/errors/not_partial.txt", CslyGeneratorErrors.NOT_PARTIAL)]
    [InlineData("/sourceGenerators/data/errors/missing_lexer.txt", CslyGeneratorErrors.LEXER_NOT_FOUND)]
    [InlineData("/sourceGenerators/data/errors/missing_parser.txt", CslyGeneratorErrors.PARSER_NOT_FOUND)]
    public void TestGeneratorError(string source, string expectedError)
    {
        string className = "ErrorGenerator";
        var code = _embeddedResourceFileSystem.ReadAllText(source);
        var result = generateSource(code, className);

        Assert.False(result.Diagnostics.IsEmpty);
        var diagnostic = result.Diagnostics.Single();
        Assert.Equal(expectedError,diagnostic.Id);
        
    }
    
    [Theory]
    [InlineData("/sourceGenerators/data/errors/missing_inheritance.txt")]
    [InlineData("/sourceGenerators/data/errors/missing_inheritance_2.txt")]
    public void TestNoGenerator(string source)
    {
        string className = "ErrorGenerator";
        var code = _embeddedResourceFileSystem.ReadAllText(source);
        var result = generateSource(code, className);

        Assert.True(result.Diagnostics.IsEmpty);
        Assert.Empty(result.GeneratedTrees);
        
        
    }
}