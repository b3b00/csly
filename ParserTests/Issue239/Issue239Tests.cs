using System.Collections.Generic;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue239
{
    public class Issue239Tests
    {
        private static Parser<Issue239Lexer, object> BuildParser()
        {   
            var StartingRule = $"statements";
            var parserInstance = new Issue239Parser();
            var builder = new ParserBuilder<Issue239Lexer, object>();
            var pBuild = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, StartingRule);
            Assert.True(pBuild.IsOk);
            Assert.NotNull(pBuild.Result);
            return pBuild.Result;
        }



        [Fact]
        public static void TestOk()
        {
            var parser = BuildParser();
            var parseResult = parser.Parse("int x; int y; a = 12;");
            Assert.True(parseResult.IsOk);
            Assert.IsAssignableFrom<List<object>>(parseResult.Result);
            var lst = parseResult.Result as List<object>;
            Assert.Equal(3, lst.Count);
        }
    }
}