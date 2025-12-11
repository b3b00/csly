using NFluent;
using sly.parser;
using sly.parser.generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ParserTests.Issue596
{
    public class TestIssue596
    {
        [Fact]
        public void Issue596Test()
        {
            var parserInstance = new GettingStartedParser();
            var builder = new ParserBuilder<GettingStartedLexer, int>();
            var buildResult = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "expression");
            Check.That(buildResult).IsOk();
            Assert.True(buildResult.IsOk);
            var parser = buildResult.Result;
            
            var result = parser.Parse("1+2+3");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsEqualTo(6);
            

            result = parser.Parse("");
            Check.That(result).Not.IsOkParsing();
            var error = result.Errors.First();
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
            Check.That(error.ErrorMessage).Contains("INT");
            
        }

    }
}
