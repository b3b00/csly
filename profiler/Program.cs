using System;
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
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"test #{i} .");
                var ignored = result.Result.Parse(content);
                Console.WriteLine($"test #{i} done.");
            }
                
            
        }
    }
}