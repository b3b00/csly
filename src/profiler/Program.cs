using System;
using System.Diagnostics;
using System.IO;
using jsonparser;
using jsonparser.JsonModel;
using sly.parser.generator;

namespace profiler
{
    class Program
    {
        static void Main(string[] args)
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