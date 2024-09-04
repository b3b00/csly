

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
using csly.indentedWhileLang.compiler;
using csly.indentedWhileLang.parser;
using csly.whileLang.interpreter;
using csly.whileLang.model;
using ParserTests.aot;
using sly.parser;



TestGenerator generator = new TestGenerator();
  

 var parserBuild = generator.GetParser();
	 if (parserBuild.IsOk)
	 {
		 Console.WriteLine("parser is ok :)");
		 var parsed = parserBuild.Result.Parse("2 add 2");
		 if (parsed.IsOk)
		 {
			 Console.WriteLine($"parse ok : {parsed.Result}");
		 }
		 else
		 {
			 foreach (var error in parsed.Errors)
			 {
				 Console.WriteLine(error);
			 }
		 }
	 }
	 else
	 {
		 foreach (var error in parserBuild.Errors)
		 {
			 Console.WriteLine(error.Message);
		 }
	 }
 
	 WhileGenerator whiley = new WhileGenerator();
	 var whileParser = whiley.GetParser();
	 if (whileParser.IsOk)
	 {
		 Console.WriteLine("while parser is ok :)");
		 var source = @"
a:=0 
while a < 10 do 
    print a
    a := a +1
";
		 var parsedwhile = whileParser.Result.Parse(source);
		 if (parsedwhile.IsOk)
		 {
			 Console.WriteLine($"parse ok : {parsedwhile.Result.Dump("  ")}");
			 Interpreter interpreter = new Interpreter();
			 interpreter.Interprete(parsedwhile.Result,false);
		 }
		 else
		 {
			 foreach (var error in parsedwhile.Errors)
			 {
				 Console.WriteLine(error);
			 }
		 }
	 }
	 else
	 {
		 foreach (var error in whileParser.Errors)
		 {
			 Console.WriteLine(error.Message);
		 }
	 }