using System;
using System.IO;
using System.Linq.Expressions;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using BravoLights.Ast;
using BravoLights.Common.Ast;
using sly.parser;
using sly.parser.generator;

namespace Issue254
{
    public class Bench
    {
         [MemoryDiagnoser]
            
            [Config(typeof(Config))]
            public class JsonParserBench
            {
        
        
                private class Config : ManualConfig
                {
                    public Config()
                    {
                        try
                        {
                            Console.WriteLine("configuring bench");
                            var baseJob = Job.MediumRun.WithToolchain(CsProjCoreToolchain.NetCoreApp50);
                            AddJob(baseJob.WithNuGet("sly", "2.7.0.5").WithId("2.7.0.5"));
                            AddJob(baseJob.WithNuGet("sly", "2.7.0.4").WithId("2.7.0.4"));
                            AddJob(baseJob.WithNuGet("sly", "2.7.0.1").WithId("2.7.0.1"));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("in Config : ");
                            Console.WriteLine(e.Message);
                        }

                    }
                }
        
                private Parser<ExpressionToken, IAstNode> BenchedParser;
        
                
        
                [GlobalSetup]
                public void Setup()
                {
                    Console.WriteLine(("SETUP"));
        
                    var parserInstance  = new MSFSExpressionParser();
                    var builder = new ParserBuilder<ExpressionToken, IAstNode>();
                    var result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "MSFSExpressionParser_expressions");
                    
                    Console.WriteLine("parser built.");
                    if (result.IsError)
                    {
                        Console.WriteLine("ERROR");
                        result.Errors.ForEach(e => Console.WriteLine(e.Message));
                    }
                    else
                    {
                        Console.WriteLine("parser ok");
                        BenchedParser = result.Result;
                    }
                    
                    Console.WriteLine($"parser {BenchedParser}");
                }
        
                [Benchmark]
                public void Test254()
                {
                    if (BenchedParser == null)
                    {
                        Console.WriteLine("parser is null");
                    }
                    else
                    {
                        var ignored = BenchedParser.Parse("-(1+2 * 3)");    
                    }
                }
        
        
        
            }
    }
}