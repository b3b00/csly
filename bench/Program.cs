using System;
using System.IO;
using System.Diagnostics;
using BenchmarkDotNet.Running;
using sly;
using sly.lexer;
using sly.buildresult;



namespace bench
{
    class Program
    {

        private static void BenchJSON() {
           
            var summary = BenchmarkRunner.Run<Bench>();

        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            BenchJSON();
        }
    }
}
