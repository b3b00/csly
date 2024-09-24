// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using handExpressions.ebnfparser;
using handExpressions.extractor;
using handExpressions.jsonparser;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using simpleExpressionParser;
using sly.lexer;
using sly.parser.generator.visitor;

namespace  handExpressions;

public class Program
{
    public static void Main(string[] args)
    {
        //TestHandParser();
         //var summary = BenchmarkRunner.Run<BenchExpressionCslyVsHand>();
         var summary = BenchmarkRunner.Run<BenchJsonCslyVsHand>();
         //PP();
         // Extract();
    }

    private static void PP()
    {
    
        var gen = new GeneratedEbnfJsonGenericParser(new EbnfJsonGenericParser());
        var source = "{\"hello\":\"world\"}";
        source = File.ReadAllText("C:/Users/olduh/dev/csly/src/bench2.4/test.json");
        var j = gen.ParseRoot(source);
        
        
        Console.WriteLine(j.ToString());
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