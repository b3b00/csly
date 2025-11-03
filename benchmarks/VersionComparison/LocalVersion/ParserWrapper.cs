using System;
using sly.lexer;
using sly.parser.generator;

namespace LocalVersion
{


    public class ParserWrapper
    {
        private sly.parser.Parser<ExpressionToken, int> _parser;

        public ParserWrapper()
        {
            var builder = new ParserBuilder<ExpressionToken, int>();
            var buildResult = builder.BuildParser(
                new ExpressionParser(),
                ParserType.EBNF_LL_RECURSIVE_DESCENT,
                "expression"
            );

            if (buildResult.IsError)
            {
                var msg = $"Failed to build parser: {string.Join(", ", buildResult.Errors)}";
                Console.Error.WriteLine(msg);
                throw new Exception();
            }

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

