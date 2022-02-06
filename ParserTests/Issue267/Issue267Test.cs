using System;
using System.Collections.Generic;
using System.Text;
using GenericLexerWithCallbacks;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

using Xunit;

namespace ParserTests.Issue267
{
    public class Issue267Test
    {
        [Theory]
        [InlineData("declare x = 1.0", "x", 1.0d)]
        [InlineData("DECLARE x = 1.0", "x", 1.0d)]
        [InlineData("DeclAre x = 1.0", "x", 1.0d)]
        [InlineData("DEClaRe x = 1.0", "x", 1.0d)]
        public void test167(string program, string expectedId, double expectedValue)
        {
            var commandParser = new Issue267Parser();
            var parserBuilder = new ParserBuilder<Issue267Token, Result167>();
            var buildResult = parserBuilder.BuildParser(commandParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "program");

            Assert.True(buildResult.IsOk);
            var lexer = buildResult.Result.Lexer as GenericLexer<Issue267Token>;
            CallBacksBuilder.BuildCallbacks(lexer);

            var parser = buildResult.Result;
            var parseResult = parser.Parse(program);
            Assert.True(parseResult.IsOk);
            
            Assert.True(parseResult.IsOk);
            var result = parseResult.Result;
            Assert.NotNull(result);
            Assert.Equal(expectedId,result.Name);
            Assert.Equal(expectedValue,result.Value);
            
        }

    }
}
