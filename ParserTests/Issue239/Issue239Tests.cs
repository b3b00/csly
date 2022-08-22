using System.Collections.Generic;
using NFluent;
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
            Check.That(pBuild.IsOk).IsTrue();
            Check.That(pBuild.Result).IsNotNull();
            return pBuild.Result;
        }



        [Fact]
        public static void TestOk()
        {
            var parser = BuildParser();
            var parseResult = parser.Parse("int x; int y; a = 12;");
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Result).IsInstanceOf<List<object>>();
            var lst = parseResult.Result as List<object>;
            Check.That(lst).CountIs(3);
        }
    }
}