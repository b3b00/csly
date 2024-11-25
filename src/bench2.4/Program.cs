using System;
using bench.json;
using bench.json.model;
using BenchmarkDotNet.Running;
using sly.parser.generator;

namespace bench
{
    class Program
    {

        private static void BenchJson() {
           
            ExpressionParserBench bench = new ExpressionParserBench();
            bench.Setup();
            bench.TestExpression();
            
            //var summary = BenchmarkRunner.Run<JsonParserBench>();
            var summary = BenchmarkRunner.Run<ExpressionParserBench>();

        }
        static void Main(string[] args)
        {
            try
            {
                
                // var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
                // var jsonParser = new EbnfJsonGenericParser();
                // var result = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
                // Console.WriteLine("parser built.");
                // if (result.IsError)
                // {
                //     Console.WriteLine("ERROR");
                //     result.Errors.ForEach(e => Console.WriteLine(e.Message));
                //     Environment.Exit(1);
                // }
                // else
                // {
                //     Console.WriteLine("parser ok");
                // }


                Console.WriteLine("Hello World!");
                BenchJson();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
