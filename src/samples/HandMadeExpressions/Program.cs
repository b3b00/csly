// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using handExpressions.ebnfparser;
using handExpressions.extractor;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using simpleExpressionParser;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;

namespace  handExpressions;

public class Program
{
    public static void Main(string[] args)
    {
//        TestHandParser();
         //var summary = BenchmarkRunner.Run<BenchCslyVsHand>();
         //PP();
         Extract();
    }

    private static void PP()
    {
        var p = new EbnfParser(new List<string>(){"hello","bonjour"});
        var r = p.ParseRule("rule : hello[d] world is? (bonjour)*");
        Console.WriteLine(r);
    }

    private static List<string> ExtractTokens(string path)
    {
         var lex = File.ReadAllText(path);
         var tree = CSharpSyntaxTree.ParseText(lex);
         var ns = tree.GetCompilationUnitRoot().Members[0] as NamespaceDeclarationSyntax;
         var e = ns.Members[0] as EnumDeclarationSyntax;
         var tokens = e.Members.Cast<EnumMemberDeclarationSyntax>().Select(x => x.Identifier.Text).ToList();
         return tokens;
    }
    
    private static void Extract()
    {
        string rootDir = "C:/Users/olduh/dev/csly/src/samples/HandMadeExpressions/json";
        var lex = File.ReadAllText(Path.Combine(rootDir,"JsonTokenGeneric.cs"));
        var tree = CSharpSyntaxTree.ParseText(lex);
        var ns = tree.GetCompilationUnitRoot().Members[0] as NamespaceDeclarationSyntax;
        var e = ns.Members[0] as EnumDeclarationSyntax;
        
        
        
        var source = File.ReadAllText(Path.Combine(rootDir,"EbnfJsonGenericParser.cs"));
        tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetCompilationUnitRoot();
        ns = root.Members[0]as NamespaceDeclarationSyntax;
        var cls = ns.Members[0] as ClassDeclarationSyntax;


        ParserGenerator generator = new ParserGenerator(e, cls, "JSon");
        var generated = generator.Generate();
        Console.WriteLine(generated);
        File.WriteAllText(Path.Combine(rootDir,"Generated.cs"), generated);
    }
    
    private static void TestHandParser()
    {
        var lexer = LexerBuilder.BuildLexer<GenericExpressionToken>();
        var t = lexer.Result.Tokenize("-1 + 1! + (true ? 42 : -42)+sum(1,2,3,4)");
        var tokens = t.Tokens.MainTokens();
        var instance = new GenericSimpleExpressionParser();
        ExpressionParser parser = new ExpressionParser(instance);
        var r = parser.Root(tokens, 0);
        if (r.Matched) {
        Console.WriteLine(r.Node.Dump("  "));
        EBNFSyntaxTreeVisitor<GenericExpressionToken, double> visitor =
            new EBNFSyntaxTreeVisitor<GenericExpressionToken, double>(null, instance);
        var result = visitor.VisitSyntaxTree(r.Node);
        Console.WriteLine(result.ToString());
        }
        else {
            Console.WriteLine("parse error!");
            }
    }
}


[MemoryDiagnoser]
[HtmlExporter]
[MarkdownExporter]
[JsonExporter("-custom", indentJson: true, excludeMeasurements: true)]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class BenchCslyVsHand()
{
    private Parser<GenericExpressionToken, double> _cslyParser;

    private string source = "1+2+3+4+5+6+7+8+9+10+11+12+13 *48! / + (true ? 42 : -42)";
    private BuildResult<ILexer<GenericExpressionToken>> _lexer;
    private int _loops = 1000;

    [GlobalSetup]
    public void Setup()
    {
        _lexer = LexerBuilder.BuildLexer<GenericExpressionToken>();
        ParserBuilder<GenericExpressionToken, double> builder = new ParserBuilder<GenericExpressionToken, double>();
        var build = builder.BuildParser(new GenericSimpleExpressionParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        _cslyParser = build.Result;
    }
    
    [Benchmark]
    public void Csly()
    {
        for (int i = 0; i < _loops; i++)
        {
            _cslyParser.Parse(source);
        }
    }
    
    [Benchmark]
    public void Hand()
    {
        for (int i = 0; i < _loops; i++)
        {
            var t = _lexer.Result.Tokenize("-1 + 1! + 4");
            var tokens = t.Tokens.MainTokens();
            var instance = new GenericSimpleExpressionParser();
            ExpressionParser parser = new ExpressionParser(instance);
            var r = parser.Root(tokens, 0);
            EBNFSyntaxTreeVisitor<GenericExpressionToken, double> visitor =
                new EBNFSyntaxTreeVisitor<GenericExpressionToken, double>(null, instance);
            var result = visitor.VisitSyntaxTree(r.Node);
        }
    }
}
