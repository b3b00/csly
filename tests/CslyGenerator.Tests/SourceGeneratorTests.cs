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

    private const string TemplateTest = @"

[ParserGenerator(typeof(TemplateLexer), typeof(TemplateParser), typeof(object))]
public partial class TemplateParserGenerator
{
        
}

 [Lexer(IgnoreEOL = true)]
    public enum TemplateLexer
    {
 
        #region TEXT
   
        [UpTo(""{%"", ""{="")]
        TEXT,
        
        [Sugar(""{%"")] [Push(""code"")] OPEN_CODE,
        
        [Sugar(""{="")] [Push(""value"")] OPEN_VALUE,

    #endregion

    #region value

    [AlphaId]
    [Mode(""value"")]
    [Mode(""code"")]
    ID,
    
    [Sugar(""=}"")]
    [Mode(""value"")]
    [Pop]
    CLOSE_VALUE,

    #endregion
    
    #region code
    
    [Sugar(""%}"")]
    [Mode(""code"")]
    [Pop]
    CLOSE_CODE,
    
    [Keyword(""if"")]
    [Mode(""code"")]
    IF,
    
    [Keyword(""endif"")]
    [Mode(""code"")]
    ENDIF,
    
    [Keyword(""else"")]
    [Mode(""code"")]
    ELSE,
    
    [Keyword(""for"")] 
    [Mode(""code"")]
    FOR,

    [Keyword(""as"")] 
    [Mode(""code"")]
    AS,
    
    [Keyword(""end"")] 
    [Mode(""code"")]
    END,
    
    [Sugar("".."")]
    [Mode(""code"")]
    RANGE,
    
    #region literals
    
    [String()]
    [Mode(""code"")]
    STRING,
    
    // [Int()]
    // [Mode(""code"")]
    // INT,
    
    [Int()]
    [Mode(""code"")]
    INT,
    
    [Lexeme(GenericToken.KeyWord, ""TRUE"")]
    [Lexeme(GenericToken.KeyWord, ""true"")]
    [Mode(""code"")]
    TRUE,

    [Lexeme(GenericToken.KeyWord, ""FALSE"")]
    [Lexeme(GenericToken.KeyWord, ""false"")]
    [Mode(""code"")]
    FALSE,
    
    
    
    #endregion
    
    #region operators 30 -> 49

    [Sugar( "">"")]
    [Mode(""code"")]
    GREATER = 30,

    [Sugar( ""<"")]
    [Mode(""code"")]
    LESSER = 31,

    [Sugar( ""=="")]
    [Mode(""code"")]
    EQUALS = 32,

    [Sugar( ""!="")]
    [Mode(""code"")]
    DIFFERENT = 33,

    [Sugar( ""&"")]
    [Mode(""code"")]
    CONCAT = 34,

    [Sugar( "":="")]
    [Mode(""code"")]
    ASSIGN = 35,

    [Sugar( ""+"")]
    [Mode(""code"")]
    PLUS = 36,

    [Sugar( ""-"")]
    [Mode(""code"")]
    MINUS = 37,


    [Sugar( ""*"")]
    [Mode(""code"")]
    TIMES = 38,

    [Sugar( ""/"")]
    [Mode(""code"")]
    DIVIDE = 39,
    
    
    #endregion
    
    #region sugar 100 -> 150
    
    [Sugar(""("")]
    [Mode(""code"")]
    OPEN_PAREN,
    
    [Sugar("")"")]
    [Mode(""code"")]
    CLOSE_PAREN,
    
    [Lexeme(GenericToken.KeyWord, ""NOT"")] [Lexeme(GenericToken.KeyWord, ""not"")]
    [Mode(""code"")]
    NOT,

    [Lexeme(GenericToken.KeyWord, ""AND"")] [Lexeme(GenericToken.KeyWord, ""and"")]
    [Mode(""code"")]
    AND,

    [Lexeme(GenericToken.KeyWord, ""OR"")] [Lexeme(GenericToken.KeyWord, ""or"")]
    [Mode(""code"")]
    OR,
    
    #endregion
    
    #endregion
        
    }


[ParserRoot(""template"")]
    public class TemplateParser
    {
        
        #region structure

        [Production(""template: item*"")]
        public object Template(List<object> items)
        {
            return null;
        }

        [Production(""item : TEXT"")]
        public object Text(Token<TemplateLexer> text)
        {
         return null;
        }
        
        [Production(""item :OPEN_VALUE[d] ID CLOSE_VALUE[d]"")]
        public object Value(Token<TemplateLexer> value)
        {
            return null;
        }

        [Production(@""item : OPEN_CODE[d] IF[d] OPEN_PAREN[d] TemplateParser_expressions CLOSE_PAREN[d] CLOSE_CODE[d]
                                     item* 
                                  elseBlock? 
                                  OPEN_CODE[d] ENDIF[d] CLOSE_CODE[d] "")]
        public object Conditional(Expression cond, List<object> thenBlock, ValueOption<object> elseBlock)
        {
            return null;
        }

        [Production(""if : OPEN_CODE[d] IF[d] OPEN_PAREN[d] TemplateParser_expressions CLOSE_PAREN[d] CLOSE_CODE[d]"")]
        public object If(object condition)
        {
            return null;
        }

        [Production(""elseBlock : OPEN_CODE[d] ELSE[d] CLOSE_CODE[d] item*"")]
        public object elseBlock(List<object> items)
        {
            return null;
        }
        

        [Production(""item : OPEN_CODE[d] FOR[d] INT RANGE[d] INT AS[d] ID CLOSE_CODE[d] item* OPEN_CODE[d] END[d] CLOSE_CODE[d]"")]
        public object fori(Token<TemplateLexer> start, Token<TemplateLexer> end, Token<TemplateLexer> iterator, List<object> items)
        {
            return null;
        }
        
        [Production(""item : OPEN_CODE[d] FOR[d] ID AS[d] ID CLOSE_CODE[d] item* OPEN_CODE[d] END[d] CLOSE_CODE[d]"")]
        public object _foreach(Token<TemplateLexer> listName, Token<TemplateLexer> iterator, List<object> items)
        {
            return null;
        }
       
        #endregion
        
        #region COMPARISON OPERATIONS

        [Infix(""LESSER"", Associativity.Right, 50)]
        [Infix(""GREATER"", Associativity.Right, 50)]
        [Infix(""EQUALS"", Associativity.Right, 50)]
        [Infix(""DIFFERENT"", Associativity.Right, 50)]
        public Expression binaryComparisonExpression(Expression left, Token<TemplateLexer> operatorToken,
            Expression right)
        {
            
            return null;
        }

        #endregion

        #region STRING OPERATIONS

        // [Operation((int) TemplateLexer.CONCAT, Affix.InFix, Associativity.Right, 10)]
        // public Expression binaryStringExpression(Expression left, Token<TemplateLexer> operatorToken, Expression right)
        // {
        //     return null;
        // }

        #endregion
        
          #region OPERANDS

          
        [Production(""primary: INT"")]
        public Expression PrimaryInt(Token<TemplateLexer> intToken)
        {
            return null;
        }

        
        [Production(""primary: TRUE"")]
        [Production(""primary: FALSE"")]
        public object PrimaryBool(Token<TemplateLexer> boolToken)
        {
            return null;
        }

        
        [Production(""primary: STRING"")]
        public object PrimaryString(Token<TemplateLexer> stringToken)
        {
            return null;
        }

        
        [Production(""primary: ID"")]
        public Expression PrimaryId(Token<TemplateLexer> varToken)
        {
            return null;
        }

        [Operand]
        [Production(""operand: primary"")]
        public Expression Operand(Expression prim)
        {
            return null;
        }

        #endregion

        #region NUMERIC OPERATIONS

        [Operation((int) TemplateLexer.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) TemplateLexer.MINUS, Affix.InFix, Associativity.Right, 10)]
        public Expression binaryTermNumericExpression(Expression left, Token<TemplateLexer> operatorToken,
            Expression right)
        {
           return null;
        }

        [Operation((int) TemplateLexer.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) TemplateLexer.DIVIDE, Affix.InFix, Associativity.Right, 50)]
        public Expression binaryFactorNumericExpression(Expression left, Token<TemplateLexer> operatorToken,
            Expression right)
        {
            return null;
        }

        [Prefix((int) TemplateLexer.MINUS, Associativity.Right, 100)]
        public Expression unaryNumericExpression(Token<TemplateLexer> operation, Expression value)
        {
            return null;
        }

        #endregion


        #region BOOLEAN OPERATIONS

        [Operation((int) TemplateLexer.OR, Affix.InFix, Associativity.Right, 10)]
        public Expression binaryOrExpression(Expression left, Token<TemplateLexer> operatorToken, Expression right)
        {
            return null;
        }

        [Operation((int) TemplateLexer.AND, Affix.InFix, Associativity.Right, 50)]
        public Expression binaryAndExpression(Expression left, Token<TemplateLexer> operatorToken, Expression right)
        {
            return null;
        }

        [Operation((int) TemplateLexer.NOT, Affix.PreFix, Associativity.Right, 100)]
        public Expression binaryOrExpression(Token<TemplateLexer> operatorToken, Expression value)
        {
            return null;
        }

        #endregion
    }


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
    
    [Fact]
    public void TemplateGeneratorTest()
    {
        var generatedTrees = testSource(TemplateTest, nameof(TemplateTest));
        
        var contents = generatedTrees.ToDictionary(x => x.FilePath,x => x.ToString());
        var generatedFiles = generatedTrees.Select(x => new FileInfo(x.FilePath).Name);
        Assert.Equivalent(new[]
        {
            "ExtendedLexerGenerator.g.cs"
        }, generatedFiles);
    }
}