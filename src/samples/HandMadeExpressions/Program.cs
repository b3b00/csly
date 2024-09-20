// See https://aka.ms/new-console-template for more information

using simpleExpressionParser;
using sly.lexer;
using sly.parser.generator.visitor;

namespace  handExpressions;

public class Program
{
    public static void Main(string[] args)
    {
        var lexer = LexerBuilder.BuildLexer<GenericExpressionToken>();
        var t = lexer.Result.Tokenize("-1 + 1! + 4");
        var tokens = t.Tokens.MainTokens();
        var instance = new GenericSimpleExpressionParser();
        ExpressionParser parser = new ExpressionParser(instance);
        var r = parser.Root(tokens, 0);
        Console.WriteLine(r.Node.Dump("  "));
        EBNFSyntaxTreeVisitor<GenericExpressionToken, double> visitor =
            new EBNFSyntaxTreeVisitor<GenericExpressionToken, double>(null, instance);
        var result = visitor.VisitSyntaxTree(r.Node);
        Console.WriteLine(result.ToString());

    }
}

