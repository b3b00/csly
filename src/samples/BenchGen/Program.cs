
// See https://aka.ms/new-console-template for more information

using sly.lexer;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.sourceGenerator.generated.ebnfparser;
using sly.sourceGenerator.generated.ebnfparser.model;
using EbnfJsonGenericParser = benchgen.jsonparser.EbnfJsonGenericParser;
using JsonTokenGeneric = benchgen.jsonparser.JsonTokenGeneric;

static void TestGen()
{
    var _source = File.ReadAllText("C:/Users/olduh/dev/csly/src/bench2.4/test.json");
    var _lexer = LexerBuilder.BuildLexer<JsonTokenGeneric>();
    var t = _lexer.Result.Tokenize(_source);
    var tokens = t.Tokens.MainTokens();
    var instance = new EbnfJsonGenericParser();
    // GeneratedEbnfJsonGenericParser p = new GeneratedEbnfJsonGenericParser(instance);
    // var rr = p.Root(tokens, 0);
    
    // GeneratedEbnfJsonGenericParser parser = new GeneratedEbnfJsonGenericParser(instance);
    // var r = parser.Root(tokens, 0);
    // EBNFSyntaxTreeVisitor<JsonTokenGeneric, JSon> visitor = new EBNFSyntaxTreeVisitor<JsonTokenGeneric, JSon>(null, instance);
    // var result = visitor.VisitSyntaxTree(r.Node);

    var rule = "hello : world* (x y Z[d])? PLUS+ 'string' 'bololo'[d]";
    
    EbnfRuleTokenizer tokenizer = new EbnfRuleTokenizer();
    var ebnfTokens = tokenizer.Tokenize(rule);
    foreach (var token in ebnfTokens) {
        Console.WriteLine(token);
    }

    EbnfRuleParser parser = new EbnfRuleParser(new List<string>(){"Z", "PLUS"});
    var r = parser.Parse(rule);
    if (r.Matched)
    {
        Console.WriteLine("OK");
        Console.WriteLine(r.Node.Dump("  "));
        EBNFSyntaxTreeVisitor<EbnfRuleToken, IGrammarNode> visitor =new EBNFSyntaxTreeVisitor<EbnfRuleToken, IGrammarNode>(null, null);
        var graphviz = new GraphVizEBNFSyntaxTreeVisitor<EbnfRuleToken,IGrammarNode>();
        var root = graphviz.VisitTree(r.Node);
        string graph = graphviz.Graph.Compile();
        var re = visitor.VisitSyntaxTree(r.Node, new NoContext());
        Console.WriteLine(re.Dump());
    }
    else
    {
        Console.WriteLine("FAIL");
    }
    
    
}

TestGen();