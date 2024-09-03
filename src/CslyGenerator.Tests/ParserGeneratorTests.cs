using System.IO;
using System.Linq;
using cslyGenerator;
using CslyGenerator.Tests.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace CslyGenerator.Tests;

public class ParserGeneratorTests
{
    private const string parserClassTest = @"
using aot.lexer;
using sly.lexer;
using sly.parser.generator;

namespace TestNamespace;

public enum AotTestLexer
{
    [Double]
    DOUBLE,
    [AlphaId]
    IDENTIFIER,
    [Sugar(""+"")]
    PLUS,
    [Sugar(""++"")]
    INCREMENT,
    [Sugar(""-"")]
    MINUS,
    [Sugar(""*"")]
    TIMES,
    [Sugar(""/"")]
    DIVIDE,
    [Sugar(""("")]
    LPAREN,
    [Sugar("")"")]
    RPAREN,
    [Sugar(""!"")]
    FACTORIAL,
 
    

    [Push(""carré"")]
    [Sugar(""²"")]
    SQUARE,
[Mode(""carré"")]
[Lexeme(""$-$"")]
REGEX,

[Lexeme(GenericToken.KeyWord,""hello"")]
HELLO,
[Lexeme(GenericToken.String)]
STR,
[Lexeme(GenericToken.Int)]
I,
}

public class AotTestParser
{
    [Production(""root : SimpleExpressionParser_expressions"")]
    public double Root(double value) => value;
        
    [Operation((int) AotTestLexer.PLUS, Affix.InFix, Associativity.Right, 10)]
    [Operation(""MINUS"", Affix.InFix, Associativity.Left, 10)]
    public double BinaryTermExpression(double left, Token<AotTestLexer> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case AotTestLexer.PLUS:
            {
                result = left + right;
                break;
            }
            case AotTestLexer.MINUS:
            {
                result = left - right;
                break;
            }
        }

        return result;
    }


    [Operation((int) AotTestLexer.TIMES, Affix.InFix, Associativity.Right, 50)]
    [Operation(""DIVIDE"", Affix.InFix, Associativity.Left, 50)]
    [NodeName(""multiplication_or_division"")]
    public double BinaryFactorExpression(double left, Token<AotTestLexer> operation, double right)
    {
        double result = 0;
        switch (operation.TokenID)
        {
            case AotTestLexer.TIMES:
            {
                result = left * right;
                break;
            }
            case AotTestLexer.DIVIDE:
            {
                result = left / right;
                break;
            }
        }

        return result;
    }


    [Prefix((int) AotTestLexer.MINUS,  Associativity.Right, 100)]
    public double PreFixExpression(Token<AotTestLexer> operation, double value)
    {
        return -value;
    }

    [Postfix((int) AotTestLexer.FACTORIAL, Associativity.Right, 100)]
    public double PostFixExpression(double value, Token<AotTestLexer> operation)
    {
        if (operation.TokenID == AotTestLexer.SQUARE)
        {
            return value * value;
        }
        if(operation.TokenID == AotTestLexer.FACTORIAL || operation.Value == ""!"")
        {
            var factorial = 1;
            for (var i = 1; i <= value; i++) factorial *= i;
            return factorial;
        }
        return value;
    }

    [Operand]
    [Production(""operand : primary_value"")]
    [NodeName(""double"")]
    public double OperandValue(double value)
    {
        return value;
    }


    [Production(""primary_value : DOUBLE"")]
    [NodeName(""double"")]
    public double OperandDouble(Token<AotTestLexer> value)
    {
        return value.DoubleValue;
    }
        
    [Production(""primary_value : INT"")]
    [NodeName(""integer"")]
    public double OperandInt(Token<AotTestLexer> value)
    {
        return value.DoubleValue;
    }

    [Production(""primary_value : LPAREN SimpleExpressionParser_expressions RPAREN"")]
    [NodeName(""group"")]
    public double OperandGroup(Token<AotTestLexer> lparen, double value, Token<AotTestLexer> rparen)
    {
        return value;
    }
}

[ParserGenerator(typeof(AotTestLexer), typeof(AotTestParser), typeof(double))]
public partial class TestGenerator
{
    
}

"; 

    [Fact]
    public void ParserGeneratorTest()
    {
        // Create an instance of the source generator.
        var generator = new CslyParserGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(new[] { generator });

        // To run generators, we can use an empty compilation.
        var compilation = CSharpCompilation.Create(nameof(ParserGeneratorTest),
            new[] { CSharpSyntaxTree.ParseText(parserClassTest) },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators. Don't forget to use the new compilation rather than the previous one.
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generatedFiles = runResult.GeneratedTrees.Select(x => new FileInfo(x.FilePath).Name).ToArray();

        Assert.Equivalent(new[]
        {
            "TestGenerator.g.cs"
        }, generatedFiles);
    }
}