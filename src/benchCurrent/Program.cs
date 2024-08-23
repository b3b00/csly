using System;
using BenchmarkDotNet.Running;

namespace benchCurrent
{
    static class Program
    {

        private static void BenchJson() {
           
            // var summary = BenchmarkRunner.Run<JsonParserBench>();
            //
            // var summary2 = BenchmarkRunner.Run<BackTrackBench>();
            
            // var summary3 = BenchmarkRunner.Run<WhileBench>();

            var summary4 = BenchmarkRunner.Run<SimpleExpressionBench>();

        }
        static void Main(string[] args)
        {
            try
            {
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
