using System.Collections.Immutable;
using System.IO;
using System.Linq;
using sly.sourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace CslyGenerator.Tests;

public class SourceGeneratorTests
{
    private const string aotParserClassTest = $@"
using aot.lexer;
using sly.lexer;
using sly.parser.generator;

namespace TestNamespace;

internal enum AotTestLexer
{{
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
[MultiLineComment(""<!--"",""-->"",channel:Channels.Main)]
        [Mode]
        COMMENT,
}}

[ParserRoot(""root"")]
[UseMemoization]
[UseBroadentokenWindow]
[AutoCloseIndentationsAttribute]
internal class AotTestParser
{{
    [Production(""root : SimpleExpressionParser_expressions"")]
    public double Root(double value) => value;
        
    [Operation((int) AotTestLexer.PLUS, Affix.InFix, Associativity.Right, 10)]
    [Operation(""MINUS"", Affix.InFix, Associativity.Left, 10)]
    public double BinaryTermExpression(double left, Token<AotTestLexer> operation, double right)
    {{
        double result = 0;
        switch (operation.TokenID)
        {{
            case AotTestLexer.PLUS:
            {{
                result = left + right;
                break;
            }}
            case AotTestLexer.MINUS:
            {{
                result = left - right;
                break;
            }}
        }}

        return result;
    }}


    [Operation((int) AotTestLexer.TIMES, Affix.InFix, Associativity.Right, 50)]
    [Operation(""DIVIDE"", Affix.InFix, Associativity.Left, 50)]
    [NodeName(""multiplication_or_division"")]
    public double BinaryFactorExpression(double left, Token<AotTestLexer> operation, double right)
    {{
        double result = 0;
        switch (operation.TokenID)
        {{
            case AotTestLexer.TIMES:
            {{
                result = left * right;
                break;
            }}
            case AotTestLexer.DIVIDE:
            {{
                result = left / right;
                break;
            }}
        }}

        return result;
    }}


    [Prefix((int) AotTestLexer.MINUS,  Associativity.Right, 100)]
    public double PreFixExpression(Token<AotTestLexer> operation, double value)
    {{
        return -value;
    }}

    [Postfix((int) AotTestLexer.FACTORIAL, Associativity.Right, 100)]
    public double PostFixExpression(double value, Token<AotTestLexer> operation)
    {{
        if (operation.TokenID == AotTestLexer.SQUARE)
        {{
            return value * value;
        }}
        if(operation.TokenID == AotTestLexer.FACTORIAL || operation.Value == ""!"")
        {{
            var factorial = 1;
            for (var i = 1; i <= value; i++) factorial *= i;
            return factorial;
        }}
        return value;
    }}

    [Operand]
    [Production(""operand : primary_value"")]
    [NodeName(""double"")]
    public double OperandValue(double value)
    {{
        return value;
    }}


    [Production(""primary_value : DOUBLE"")]
    [NodeName(""double"")]
    public double OperandDouble(Token<AotTestLexer> value)
    {{
        return value.DoubleValue;
    }}
        
    [Production(""primary_value : INT"")]
    [NodeName(""integer"")]
    public double OperandInt(Token<AotTestLexer> value)
    {{
        return value.DoubleValue;
    }}

    [Production(""primary_value : LPAREN SimpleExpressionParser_expressions RPAREN"")]
    [NodeName(""group"")]
    public double OperandGroup(Token<AotTestLexer> lparen, double value, Token<AotTestLexer> rparen)
    {{
        return value;
    }}
}}

[ParserGenerator(typeof(AotTestLexer), typeof(AotTestParser), typeof(double))]
internal partial class TestGenerator
{{
    
}}";
    
    private const string extendedLexerTest = $@"

 internal enum ExtendedLexer 
    {{
        [Extension]
        EXT,
        
        [Sugar(""-"")]
        DASH,
        
        [AlphaNumId]
        ID,

        [Lexeme(GenericToken.KeyWord,channel:0, ""billy"", ""bob"")]
        BILLY_BOB,

        [Mode(""foo_mode"", ""bar_mode"")]
        [Lexeme(GenericToken.KeyWord,sly.lexer.Channels.Main, ExtendedLexerParser.FOO, ExtendedLexerParser.BAR)]
        FOO_BAR,
    
        
        [Lexeme(GenericToken.KeyWord,""BAZ"")]
        BAZ,

        [Mode(""qux_mode"")]
        [Lexeme(GenericToken.KeyWord, 550, ""QUX"")]
        QUX,

        [Lexeme(GenericToken.KeyWord,channel:0, ""true"", ""false"")]
        BOOLEAN,

[String]
STR,
        
    }}

    internal class ExtendedLexerParser {{

    public static string FOO = ""foo"";

        public const string BAR = ""bar"";

        int toto = 0;

        public ExtendedLexerParser() {{
        }}
        
        public void do() {{
            toto = 42;
        }}
     }}

    [ParserGenerator(typeof(ExtendedLexer),typeof(ExtendedLexerParser),typeof(string))]
    public partial class ExtendedLexerGenerator : AbstractParserGenerator<ExtendedLexer>
    {{
        public override Action<ExtendedLexer, LexemeAttribute, GenericLexer<ExtendedLexer>> UseTokenExtensions()
        {{
            var e = (ExtendedLexer token, LexemeAttribute lexem, GenericLexer<ExtendedLexer> lexer) =>
            {{
                if (token == ExtendedLexer.EXT)
                {{
                    NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
                    {{
                        match.Properties[GenericLexer<ExtendedLexer>.DerivedToken] = ExtendedLexer.EXT;
                        return match;
                    }};

                    var fsmBuilder = lexer.FSMBuilder;


                    fsmBuilder.GoTo(GenericLexer<ExtendedLexer>.start) 
                        .Transition('$')
                        .Transition('_')
                        .Transition('$')
                        .End(GenericToken.Extension) // mark as ending node 
                        .CallBack(callback); // set the ending callback
                }}
            }};
            return e;
        }}
    }}



"; 

    private ImmutableArray<SyntaxTree> testSource(string source, string className)
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
    
    
    [Fact]
    public void AotParserGeneratorTest()
    {
        var generatedTrees = testSource(aotParserClassTest, nameof(aotParserClassTest));
        
        var contents = generatedTrees.ToDictionary(x => x.FilePath,x => x.ToString());
        var generatedFiles = generatedTrees.Select(x => new FileInfo(x.FilePath).Name);
        Assert.Equivalent(new[]
        {
            "TestGenerator.g.cs"
        }, generatedFiles);
    }
    
    [Fact]
    public void ExtendedLexerGeneratorTest()
    {
        var generatedTrees = testSource(extendedLexerTest, nameof(extendedLexerTest));
        
        var contents = generatedTrees.ToDictionary(x => x.FilePath,x => x.ToString());
        var generatedFiles = generatedTrees.Select(x => new FileInfo(x.FilePath).Name);
        Assert.Equivalent(new[]
        {
            "ExtendedLexerGenerator.g.cs"
        }, generatedFiles);
    }
}