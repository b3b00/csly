using System;
using System.Collections.Generic;
using System.Text;

using sly.parser;
using sly.parser.generator;

using Xunit;

namespace ParserTests.Issue263
{
    public class Issue263Test
    {
        [Fact]
        public void OperationCanBeParsed()
        {
            var source = "()";

            var commandParser = new Issue263Parser();
            var parserBuilder = new ParserBuilder<Issue263Token, object>();
            var buildResult = parserBuilder.BuildParser(commandParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "operation");

            Assert.True(buildResult.IsOk);

            var parser = buildResult.Result;
            var parseResult = parser.Parse(source);

            Assert.True(parseResult.IsOk);
        }

        [Fact]
        public void OperationCannotBeParsedAndReturnsError()
        {
            var source = "(]";

            var commandParser = new Issue263Parser();
            var parserBuilder = new ParserBuilder<Issue263Token, object>();
            var buildResult = parserBuilder.BuildParser(commandParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "operation");

            Assert.True(buildResult.IsOk);

            var parser = buildResult.Result;
            var parseResult = parser.Parse(source);

            Assert.False(parseResult.IsOk);
            Assert.Single(parseResult.Errors);
            var error = parseResult.Errors[0];
            Assert.Equal(ErrorType.UnexpectedToken, error.ErrorType);
        }

        [Fact]
        public void OperationCannotBeParsedAndReturnsError2()
        {
            var source = "()()(]";

            var commandParser = new Issue263Parser();
            var parserBuilder = new ParserBuilder<Issue263Token, object>();
            var buildResult = parserBuilder.BuildParser(commandParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "operation");

            Assert.True(buildResult.IsOk);

            var parser = buildResult.Result;
            var parseResult = parser.Parse(source);

            Assert.False(parseResult.IsOk);
            Assert.Single(parseResult.Errors);
            var error = parseResult.Errors[0];
            Assert.Equal(ErrorType.UnexpectedToken, error.ErrorType);
        }
    }
}
