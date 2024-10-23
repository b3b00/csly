using System;
using BenchmarkDotNet.Running;

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
            var summary5 = BenchmarkRunner.Run<JsonStringEscapingBench>();

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
