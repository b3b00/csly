using System;
using BenchmarkDotNet.Running;

namespace bench
{
    class Program
    {

        private static void BenchJson() {
           
            var summary = BenchmarkRunner.Run<JsonParserBench>();

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
