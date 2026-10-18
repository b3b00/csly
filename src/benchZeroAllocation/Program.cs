using System;
using BenchmarkDotNet.Running;

namespace benchZeroAllocation
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var summary = BenchmarkRunner.Run<JsonZeroAllocationBench>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
