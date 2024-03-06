// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using ParserTests.Issue414;
using sly.parser;
using sly.parser.generator;

public class Config : ManualConfig
{
    public Config()
    {
        AddJob(new Job(Job.Dry)
        {
            Environment = { Jit = Jit.RyuJit, Platform = Platform.X64 },
            Run = { LaunchCount = 3, WarmupCount = 5, IterationCount = 10 },
            Accuracy = { MaxRelativeError = 0.01 }
        });
    }
}

public class Bench
{
    private Parser<Issue414Token, string> Parser;
    
    string source = "funcA(funcC(B==2));";
    public Bench()
    {
        var parserInstance = new Issue414ExpressionParser();
        var builder = new ParserBuilder<Issue414Token, string>();
        var buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "block");//line-based, 1 statement per line.
        Parser = buildResult.Result;
    }

    [Benchmark]
    public void NoOPtim()
    {
        var result = Parser.Parse(source,optimizeRulesIteration:false);
    }

    [Benchmark]
    public void Optim()
    {
        var result = Parser.Parse(source,optimizeRulesIteration:true);
    }

}

public class Program
{
    public static void Main(string[] args)
    {
        
        var c = new ManualConfig();
        c.Add(Job.Dry.WithLaunchCount(3).WithWarmupCount(5).WithIterationCount(10));
        c.Add(DefaultConfig.Instance.GetExporters().ToArray());
        c.Add(DefaultConfig.Instance.GetLoggers().ToArray());
        c.Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
        BenchmarkDotNet.Running.BenchmarkRunner.Run<Bench>(c);
        Console.ReadLine();
        
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly,new Config());
        Console.WriteLine(string.Join("\n",summary.Select(x => x.ToString())));
    } 
    
}

