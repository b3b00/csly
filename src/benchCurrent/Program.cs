using System;
using BenchmarkDotNet.Running;
using sly.parser.generator;

namespace benchCurrent
{
    static class Program
    {

        private static void Bench() {
           
            // var summary = BenchmarkRunner.Run<JsonParserBench>();
            //
            // var summary2 = BenchmarkRunner.Run<BackTrackBench>();
            
            // var summary3 = BenchmarkRunner.Run<WhileBench>();

            //var summary4 = BenchmarkRunner.Run<SimpleExpressionBench>();
            
            // StackExpressionBench bench = new StackExpressionBench();
            // bench.parserType = ParserType.LL_RECURSIVE_DESCENT;
            // bench.Setup();
            // bench.BenchLargeExpression();
            
            var summary5 = BenchmarkRunner.Run<StackExpressionBench>();

        }
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello World!");
                Bench();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
