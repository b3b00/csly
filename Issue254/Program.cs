using System;
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
            var c = new Stopwatch();
            c.Start();
            var parserInstance = new MSFSExpressionParser();
            var builder = new ParserBuilder<ExpressionToken, IAstNode>();
            var parser = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "logicalExpression");

            var x = parser.Result.Parse("-(1+2 * 3)");
            c.Stop();
            Console.WriteLine(c.ElapsedMilliseconds+" ms");
        }
    }
}