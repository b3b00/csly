using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BravoLights.Ast;
using BravoLights.Common.Ast;
using sly.parser.generator;

namespace Issue254
{
    public static class Program
    {

        
        
        public static void Main(string[] args)
        {
            var tests = new Dictionary<string, double>()
            {
                {"1 + 2 * 3", 7.0},
                {"2 - 3 / 4", 1.25},
                {"-3 - -4", 1.0},
                {"-3--4", 1.0},
                {"-3+-4", -7.0},
                {"-(1+2)", -3.0},
                {"-(1+2 * 3)", -7.0},
                {"3 * -2", -6.0},
                {"9 & 8", 8.0},
                {"7 & 8", 0.0},
                {"8 & 8", 8.0},
                {"1 + 7 & 15 - 7", 8.0}, 
                {"1 | 2", 3.0},
                {"3 | 5", 7.0},
                {"1 + 3 | 3 - 1", 6.0}
            };
            
            var c = new Stopwatch();
            
            var parserInstance = new MSFSExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, IAstNode>();
            var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "MSFSExpressionParser_expressions");

            foreach (var test in tests)
            {
                c.Reset();
                c.Start();
                var x = parser.Result.Parse(test.Key);
                bool isok = x.IsOk;
                c.Stop();
                Console.WriteLine($"test {test.Key} : {isok} : {c.ElapsedMilliseconds} ms");
            }

            ;

        }
    }
}