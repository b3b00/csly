

using aot;
using aot.lexer;
using aot.parser;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using CommandLine;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using ParserTests.aot;
using sly.lexer;
using sly.parser;



TestGenerator generator = new TestGenerator();
generator.GetParser();



AotIndentedWhileParserBuilder builder = new AotIndentedWhileParserBuilder();

var b = builder.BuildAotWhileParser();
var p = b.BuildParser();
if (!p.IsOk)
{
	return;
}

bool benchmark = false;

if (args.Length > 0)
{
	benchmark = args.Contains("bench");
}



if (benchmark)
{


	var summary = BenchmarkRunner.Run<Bencher>();

}
else
{
	AotIndentedWhileParserBuilder testBuilder = new AotIndentedWhileParserBuilder();
	TestAotParserBuilder aotBuilder = new TestAotParserBuilder();
	var pp = aotBuilder.FluentInitializeCenericLexer();

	// var bb = testBuilder.BuildAotWhileParser();
	// var pp = bb.BuildParser();
	if (!pp.IsOk)
	{
		foreach (var error in pp.Errors)
		{
			Console.WriteLine(error);
		}
		Environment.Exit(12);
	}

	var _parser = pp.Result;
	var r = _parser.Parse(@"10²");
	if (r.IsOk)
	{
		Console.WriteLine($"OK : {r.Result}");
	}
	else
	{
		foreach (var error in r.Errors)
		{
			Console.WriteLine(error);
		}
	}
}

// var bencher = new WhileBench();
// Console.WriteLine("AOT");
// bencher.Bench(bencher.BenchAot);
// Console.WriteLine("Legacy");
// bencher.Bench(bencher.BenchLegacy);

[SimpleJob(RuntimeMoniker.Net80, baseline:true)]
[SimpleJob(RuntimeMoniker.NativeAot80)]
[MemoryDiagnoser]
[RPlotExporter]
public class Bencher
{

	private Parser<IndentedWhileTokenGeneric, WhileAST>? _parser;

	[GlobalSetup]
	
	public void Setup()
	{
		AotIndentedWhileParserBuilder builder = new AotIndentedWhileParserBuilder();

		var b = builder.BuildAotWhileParser();
		var p = b.BuildParser();
		if (!p.IsOk)
		{
			Environment.Exit(12);
		}

		_parser = p.Result;
	}

	public Bencher()
	{
		_parser = null;
	}
	
	[Benchmark]
	public void ParseWhile()
	{
		string program = @"
a:=0
while a < 10 do 
	print a
	a := a +1
if a > 145 then
	print a
	skip
	a := a *478
	if a > 125874 then
		a := 1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17+18+19+20
		b := true
		print ""bololo""
	else
		b := false
		print ""youpi""
else
	print b
";
		var r = _parser?.Parse(program);
	}
}	