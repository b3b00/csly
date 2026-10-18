using System;
using System.IO;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using jsonparser;
using jsonparser.JsonModel;
using sly.parser;
using sly.parser.generator;

namespace benchZeroAllocation
{

    /// <summary>
    /// Demonstrates the <see cref="ZeroAllocationAttribute"/> pattern end-to-end:
    /// two parser classes derived from the same base, one decorated with
    /// <see cref="ZeroAllocationAttribute"/> and one not.
    /// <see cref="EBNFParserBuilder"/> detects the attribute via reflection and
    /// sets <see cref="ParserConfiguration{IN, OUT}.UseZeroAllocation"/> accordingly.
    /// </summary>

    [MemoryDiagnoser]
    [Config(typeof(Config))]
    public class JsonZeroAllocationBench
    {

        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.MediumRun.With(CsProjCoreToolchain.NetCoreApp80);
                Add(baseJob.WithId("net8.0"));
                Add(EnvironmentAnalyser.Default);
            }
        }

        private Parser<JsonTokenGeneric, JSon> StandardParser;
        private Parser<JsonTokenGeneric, JSon> ZeroAllocationParser;
        private string content = "";

        [Params(true, false)]
        public bool Memoize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            content = File.ReadAllText("test.json");

            // --- Standard parser: no [ZeroAllocation] attribute ---
            var standardBuilder = new ParserBuilder<JsonTokenGeneric, JSon>();
            var standardResult = standardBuilder.BuildParser(
                new StandardJsonParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            if (standardResult.IsError)
            {
                standardResult.Errors.ForEach(e => Console.WriteLine(e));
            }
            else
            {
                StandardParser = standardResult.Result;
            }

            // --- Zero-allocation parser: [ZeroAllocation] attribute applied ---
            var zallocBuilder = new ParserBuilder<JsonTokenGeneric, JSon>();
            var zallocResult = zallocBuilder.BuildParser(
                new ZeroAllocationJsonParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
            if (zallocResult.IsError)
            {
                zallocResult.Errors.ForEach(e => Console.WriteLine(e));
            }
            else
            {
                ZeroAllocationParser = zallocResult.Result;
            }
        }

        /// <summary>
        /// Parse using the standard parser (no ZeroAllocation attribute).
        /// </summary>
        [Benchmark(Baseline = true)]
        public void TestJsonParseStandard()
        {
            if (StandardParser == null) return;
            StandardParser.Configuration.UseMemoization = Memoize;
            var ignored = StandardParser.Parse(content);
        }

        /// <summary>
        /// Parse using the zero-allocation parser (ZeroAllocation attribute applied).
        /// </summary>
        [Benchmark]
        public void TestJsonParseZeroAllocation()
        {
            if (ZeroAllocationParser == null) return;
            ZeroAllocationParser.Configuration.UseMemoization = Memoize;
            var ignored = ZeroAllocationParser.Parse(content);
        }

    }

    #region Parser classes demonstrating the attribute pattern

    /// <summary>
    /// Standard JSON parser without the <see cref="ZeroAllocationAttribute"/>.
    /// </summary>
    [ParserRoot("root")]
    public class StandardJsonParser : EbnfJsonGenericParser
    {
    }

    /// <summary>
    /// JSON parser with the <see cref="ZeroAllocationAttribute"/> applied.
    /// <see cref="EBNFParserBuilder"/> detects this via reflection and sets
    /// <see cref="ParserConfiguration{IN, OUT}.UseZeroAllocation = true"/>.
    /// </summary>
    [ZeroAllocation]
    [ParserRoot("root")]
    public class ZeroAllocationJsonParser : EbnfJsonGenericParser
    {
    }

    #endregion

}
