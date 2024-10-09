
// See https://aka.ms/new-console-template for more information

using benchgen;
using benchgen.jsonparser.JsonModel;
using BenchmarkDotNet.Running;
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
    
    EbnfJsonGenericParserGenerator generator = new EbnfJsonGenericParserGenerator(new EbnfJsonGenericParser());

    
        var jsongen = generator.ParseRoot(_source);
    
        ParserBuilder<JsonTokenGeneric, JSon> builder = new ParserBuilder<JsonTokenGeneric, JSon>();
        var build = builder.BuildParser(new EbnfJsonGenericParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");

        var jsoncsly = build.Result.Parse(_source);

}

static void Bench()
{
    BenchmarkRunner.Run<BenchJsonCslyVsHand>();
}

TestGen();
//Bench();