using System;
using System.IO;
using System.Diagnostics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using sly;
using sly.lexer;
using sly.buildresult;



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
