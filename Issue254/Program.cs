using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BravoLights.Ast;
using BravoLights.Common.Ast;
using sly.parser.generator;
using sly.parser.generator.visitor;

namespace Issue254
{
    public static class Program
    {

        public static void Bench()
        {
            Console.WriteLine("starting bench");
            // var summary = BenchmarkRunner.Run<Bench>();
        }
        
        public static void Main(string[] args)
        {
            // Test254();
            Test254Old();
            // try
            // {
            //     Bench();
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e.Message);
            //     Console.WriteLine(e.StackTrace);
            // }

            ;
        }

        private static void Test254()
        {
            var tests = new Dictionary<string, double>()
            {
                // {"1 + 2 * 3", 7.0},
                // {"2 - 3 / 4", 1.25},
                // {"-3 - -4", 1.0},
                // {"-3--4", 1.0},
                // {"-3+-4", -7.0},
                // {"-(1+2)", -3.0},
                // { "-(1+2 * 3)", -7.0 } ,
                // {"3 * -2", -6.0},
                // {"9 & 8", 8.0},
                // {"7 & 8", 0.0},
                // {"8 & 8", 8.0},
                {"1 + 7 & 15 - 7", 8.0}
                // , 
                // {"1 | 2", 3.0},
                // {"3 | 5", 7.0},
                // {"1 + 3 | 3 - 1", 6.0}
            };

            var c = new Stopwatch();

            

            foreach (var test in tests)
            {
                var parserInstance = new MSFSExpressionParser();
                var builder = new ParserBuilder<ExpressionToken, IAstNode>();
                var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT,
                    "MSFSExpressionParser_expressions");
                File.WriteAllText(@"c:\temp\parser.dump.txt",parser.Result.Configuration.Dump());
                c.Reset();
                c.Start();
                var x = parser.Result.Parse(test.Key);
                bool isok = x.IsOk;
                c.Stop();
                Console.WriteLine($"test {test.Key} : {isok} : {c.ElapsedMilliseconds} ms");
                var result = parser.Result.Parse("2 + 2 * 3");
                var tree = result.SyntaxTree;
                var graphviz = new GraphVizEBNFSyntaxTreeVisitor<ExpressionToken>();
                var root = graphviz.VisitTree(tree);
                string graph = graphviz.Graph.Compile();
                File.Delete("c:\\temp\\tree.dot");
                File.AppendAllText("c:\\temp\\tree.dot", graph);
            }
        }
        
        private static void Test254Old()
        {
            var tests = new Dictionary<string, double>()
            {
                // {"1 + 2 * 3", 7.0},
                // {"2 - 3 / 4", 1.25},
                // {"-3 - -4", 1.0},
                // {"-3--4", 1.0},
                // {"-3+-4", -7.0},
                // {"-(1+2)", -3.0},
                 { "-(1+2 * 3)", -7.0 } ,
                // {"3 * -2", -6.0},
                // {"9 & 8", 8.0},
                // {"7 & 8", 0.0},
                // {"8 & 8", 8.0},
                //{"1 + 7 & 15 - 7", 8.0}
                // , 
                // {"1 | 2", 3.0},
                // {"3 | 5", 7.0},
                // {"1 + 3 | 3 - 1", 6.0}
            };

            var c = new Stopwatch();

            

            foreach (var test in tests)
            {
                var parserInstance = new OldMSFSExpressionParser();
                var builder = new ParserBuilder<OldExpressionToken, IAstNode>();
                var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT,
                    "logicalExpression");
                File.WriteAllText(@"c:\temp\parser.dump.old.txt",parser.Result.Configuration.Dump());
                c.Reset();
                c.Start();
                var x = parser.Result.Parse(test.Key);
                bool isok = x.IsOk;
                c.Stop();
                Console.WriteLine($"test {test.Key} : {isok} : {c.ElapsedMilliseconds} ms");
                var result = parser.Result.Parse("2 + 2 * 3");
                var tree = result.SyntaxTree;
                var graphviz = new GraphVizEBNFSyntaxTreeVisitor<OldExpressionToken>();
                var root = graphviz.VisitTree(tree);
                string graph = graphviz.Graph.Compile();
                File.Delete("c:\\temp\\tree.old.dot");
                File.AppendAllText("c:\\temp\\tree.dot", graph);
            }
        }
    }
}