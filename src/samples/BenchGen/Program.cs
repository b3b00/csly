
// See https://aka.ms/new-console-template for more information

using benchgen;
using benchgen.jsonparser.JsonModel;
using benchgen.jsonparser;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator.visitor;
using EbnfJsonGenericParser = benchgen.jsonparser.EbnfJsonGenericParser;
using JsonTokenGeneric = benchgen.jsonparser.JsonTokenGeneric;

static void TestGen()
{
    var _source = File.ReadAllText("C:/Users/olduh/dev/csly/src/bench2.4/test.json");
    var _lexer = LexerBuilder.BuildLexer<JsonTokenGeneric>();
    var t = _lexer.Result.Tokenize(_source);
    var tokens = t.Tokens.MainTokens();
    var instance = new EbnfJsonGenericParser();
    benchgen.jsonparser.GeneratedEbnfJsonGenericParser parser = new benchgen.jsonparser.GeneratedEbnfJsonGenericParser(instance);
    // var r = parser.Root(tokens, 0);
    // EBNFSyntaxTreeVisitor<JsonTokenGeneric, JSon> visitor =
    //     new EBNFSyntaxTreeVisitor<JsonTokenGeneric, JSon>(null, instance);
    // var result = visitor.VisitSyntaxTree(r.Node);
}

TestGen();