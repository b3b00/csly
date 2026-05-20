using BenchCslies;
using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        // CsliesBench bench = new CsliesBench();
        // bench.Setup();
        // bench.TestCsly();
        
        // ExprBenchCslies b = new ExprBenchCslies();
        // b.Setup();
        // b.TestCsly();
        // b.TestFluent();
        // b.TestGenerated();
        // Console.ReadLine();
        var summaryJson = BenchmarkRunner.Run<CsliesBench>();
        //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

