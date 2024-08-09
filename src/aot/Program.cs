

using aot.lexer;
using aot.parser;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CommandLine;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using ParserTests.aot;
using sly.parser;


AotIndentedWhileParserBuilder builder = new AotIndentedWhileParserBuilder();

var b = builder.BuildAotWhileParser();
var p = b.BuildParser();
if (!p.IsOk)
{
	return;
}

var config = DefaultConfig.Instance
	.With(Job.Default.With(NativeAotRuntime.Net80)); // compiles the benchmarks as net8.0 and uses the latest NativeAOT to build a native app

BenchmarkSwitcher
	.FromAssembly(typeof(Program).Assembly)
	.Run(args, config);

var summary = BenchmarkRunner.Run<Bencher>();

public class Bencher
{
	
	private Parser<IndentedWhileTokenGeneric, WhileAST> _parser;

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
		a := 7
		b := true
		print ""bololo""
	else
		b := false
		print ""youpi""
else
	print b
";
		var r = _parser.Parse(program);
	}
}	