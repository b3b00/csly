using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using csly.indentedWhileLang.parser;
using csly.whileLang.model;
using expressionparser;
using jsonparser;
using jsonparser.JsonModel;
using simpleExpressionParser;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using ExpressionToken = simpleExpressionParser.ExpressionToken;

namespace profiler
{
    class Program
    {

        static void ProfileWhile()
        {
            string source = @"
r:=1
i:=1
while i < 11 do 
    r := r * i
    print r
    print i
    i := i + 1
b := false 
if i == 589 then
	r2:=1
	i2:=1
	while i < 11 do 
		r2 := r2 * i2
		print r2
		print i2
		i2 := i2 + 1
	b2 := false 
	if i2 == 589 then
		r3:=1
		i3:=1
		while i3 < 11 do 
			r3 := r3 * i3
			print r3
			print i3
			i3 := i3 + 1
		b3 := false 
		if i3 == 589 then
			r4:=1
			i4:=1
			while i4 < 11 do 
				r4 := r4 * i4
				print r4
				print i4
				i4 := i4 + 1
			b4 := false 
			if i4 == 589 then
				r5:=1
				i5:=1
				while i5 < 11 do 
					r5 := r5 * i5
					print r5
					print i5
					i5 := i5 + 1
				b5 := false 
				if i5 == 589 then
					r6:=1
					i6:=1
					while i6 < 11 do 
						r6 := r6 * i6
						print r6
						print i6
						i6 := i6 + 1
					b6 := false 
					if i6 == 589 then			
						r7:=1
						i7:=1
						while i7 < 11 do 
							r7 := r7 * i7
							print r7
							print i7
							i7 := i7 + 1
						b7 := false 
						if i7 == 589 then
							print ""youpalou""

";
            
            var backTrackParser = new IndentedWhileParserGeneric();
            var builder = new ParserBuilder<IndentedWhileTokenGeneric, WhileAST>();
            
            var result = builder.BuildParser(backTrackParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");
            
            if (result.IsError)
            {
                result.Errors.ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("parser ok");
                var r = result.Result.Parse(source);
                if (r.IsError)
                {
                    foreach (var error in r.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                else
                {
	                Console.WriteLine("parse ok");
	                //Console.WriteLine(r.Result.Dump("    "));
                }

            }
        }
        
        static void Main(string[] args)
        {
	        ProfileExpressions(10000, 100, true);
            //ProfileJson();
            // for (int i = 0; i < 15; i++)
            // {
            //     ProfileWhile();
            // }
        }

        static void ProfileExpressions(int max = 10000, int step = 100, bool progression=false)
        {
	        var instance = new ExpressionParser();
	        ParserBuilder<expressionparser.ExpressionToken, int> builder =
		        new ParserBuilder<expressionparser.ExpressionToken, int>();
	        
	        var b = builder.BuildParser(instance, ParserType.LL_STACK, "expression");
	        if (b.IsError)
	        {
		        foreach (var error in b.Errors)
		        {
			        Console.WriteLine(error.Message);
		        }
		        return;
	        }

	        List<(int count, long time)> results = new();
	        
	        if (progression)
	        {
		        for (int i = 2; i < max; i += step)
		        {
			        
			        Stopwatch watch = new Stopwatch();
			        watch.Start();
			        SingleExpressionProfile(i,b);
			        watch.Stop();
			        results.Add((i, watch.ElapsedMilliseconds));
			        Console.WriteLine($"{i}: {watch.ElapsedMilliseconds} ms");
			        File.AppendAllText("c:/tmp/progression.csv", $"{i};{watch.ElapsedMilliseconds}\n");
		        }  
		        
	        }
	        else
	        {
		        SingleExpressionProfile(max, b);
	        }
        }

        private static void SingleExpressionProfile(int max, BuildResult<Parser<expressionparser.ExpressionToken, int>> b)
        {
	        var rnd = new Random();
	        //int width = rnd.Next(100, max);
	        char[] ops = new[] { '+', '-', '*' };
	        var getOp = () => ops[rnd.Next(0, ops.Length)];
	        var expression = rnd.Next(0, 100).ToString();
	        for(int i = 0; i < max; i++)
	        {
		        var op = getOp();
		        var right = rnd.Next(0, 100);
		        expression += $"{op} {right}";
		       
	        }
	        //Console.WriteLine($"parsing {expression}");
	        var result = b.Result.Parse(expression);
	        if (result.IsError)
	        {
		        File.WriteAllLines("c:/progress_errors.txt", new[] { expression });
		        File.WriteAllLines("c:/progress_errors.txt", result.Errors.Select(x => x.ErrorMessage).ToArray() );
		        Console.WriteLine($"error parsing {expression}");
		        Environment.Exit(max);

		        return;
	        }
	        Console.WriteLine("OK");
        }

        private static void ProfileJson()
        {
            Console.WriteLine(("SETUP"));
            
            var content = File.ReadAllText("test.json");
            Console.WriteLine("json read.");
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            
            var result = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            Console.WriteLine("parser built.");
            Stopwatch chrono = new Stopwatch();
            long runningTime = 0;
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"test #{i} .");
                chrono.Reset();
                chrono.Start();
                var ignored = result.Result.Parse(content);
                chrono.Stop();
                runningTime += chrono.ElapsedMilliseconds;
                Console.WriteLine($"test #{i} done. {chrono.ElapsedMilliseconds} ms");
            }
                
            Console.WriteLine($"total {runningTime} ms");
            
        }
    }
}