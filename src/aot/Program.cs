﻿

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


AotIndentedWhileParserBuilder builder = new AotIndentedWhileParserBuilder();

var b = builder.BuildAotWhileParser();
var p = b.BuildParser();
if (!p.IsOk)
{
	return;
}

// var config = DefaultConfig.Instance
// 	.With(Job.Default.With(NativeAotRuntime.Net80)); // compiles the benchmarks as net8.0 and uses the latest NativeAOT to build a native app
//
// var baseJob = Job.MediumRun.With(CsProjCoreToolchain.Current.Value);
// Add(baseJob.WithNuGet("sly", "2.2.5.1").WithId("2.2.5.1"));
// Add(baseJob.WithNuGet("sly", "2.3.0").WithId("2.3.0"));
// Add(baseJob.WithNuGet("sly", "2.4.0.1").WithId("2.4.0.1"));
// Add(EnvironmentAnalyser.Default);

//var summary = BenchmarkRunner.Run<Bencher>();

var bencher = new WhileBench();
Console.WriteLine("AOT");
bencher.Bench(bencher.BenchAot);
Console.WriteLine("Legacy");
bencher.Bench(bencher.BenchLegacy);

[SimpleJob(RuntimeMoniker.Net80, baseline:true)]
[SimpleJob(RuntimeMoniker.NativeAot80)]
[MemoryDiagnoser]
[RPlotExporter]
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
		a := 1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17+18+19+20
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