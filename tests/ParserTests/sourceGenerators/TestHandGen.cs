using System;
using System.Reflection;
using handExpressions.sourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SharpFileSystem.FileSystems;
using Xunit;

namespace ParserTests.sourceGenerators;

public class TestHandGen
{
    private EmbeddedResourceFileSystem _embeddedResourceFileSystem;
    public TestHandGen()
    {
        _embeddedResourceFileSystem = new(Assembly.GetExecutingAssembly());
    }
    
    [Fact]
    public void TestExpr()
    {
        var code = _embeddedResourceFileSystem.ReadAllText("/sourceGenerators/data/expression_hand.txt");
        
        var generator = new CslyParserGen();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(new[] { generator });

        // To run generators, we can use an empty compilation.
        var compilation = CSharpCompilation.Create("ExpressionGenerated",
            new[] { CSharpSyntaxTree.ParseText(code) },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators. Don't forget to use the new compilation rather than the previous one.
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        Console.WriteLine(runResult.ToString());
        
    }
    
    
    [Fact]
    public void TestJson()
    {
        var code = _embeddedResourceFileSystem.ReadAllText("/sourceGenerators/data/json_hand.txt");
        
        var generator = new CslyParserGen();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(new[] { generator });

        // To run generators, we can use an empty compilation.
        var compilation = CSharpCompilation.Create("ExpressionGenerated",
            new[] { CSharpSyntaxTree.ParseText(code) },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators. Don't forget to use the new compilation rather than the previous one.
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        Console.WriteLine(runResult.ToString());
        
    }
        
}