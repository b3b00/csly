using System;
using sly.parser.generator;
using System.Linq;

namespace NuGetVersion
{
    public class ParserWrapper
    {
        private sly.parser.Parser<ExpressionToken, int> _parser;

        public ParserWrapper()
        {
            Console.WriteLine("building parser");
            var builder = new ParserBuilder<ExpressionToken, int>();
            var buildResult = builder.BuildParser(
                new ExpressionParser(),
                ParserType.EBNF_LL_RECURSIVE_DESCENT,
                "expression"
            );

            Console.WriteLine($"parser built ? {buildResult.IsOk}");
            if (buildResult.IsError)
            {

                var msg = $"Failed to build parser: {string.Join(", ", buildResult.Errors.Select(x => x.Message))}";
                Console.Error.WriteLine(msg);
                throw new Exception(msg);
            }
            Console.WriteLine("Parser really built");
            _parser = buildResult.Result;
        }

        public void Parse(string input)
        {
            var result = _parser.Parse(input);
            if (result.IsError)
            {
                var msg = $"Parse error: {string.Join(", ", result.Errors)}";
                Console.Error.WriteLine(msg);
                throw new Exception(msg);
            }
        }
    }
}

