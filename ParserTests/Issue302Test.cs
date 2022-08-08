using System;
using System.Linq;
using expressionparser;
using ParserTests.Issue302;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests
{
    public class Issue302Test
    {
        [Fact]
        public void Test302()
        {
            var parse_inst = new Issue302Parser();
            var builder = new ParserBuilder<Issue302Token, object>();
            var parser = builder.BuildParser(parse_inst, ParserType.EBNF_LL_RECURSIVE_DESCENT, "expr");

            var r = parser.Result.Parse("ba + bb",nameof(Issue302Parser)+"_expressions");
            
            Assert.True(r.IsOk);
            
            r = parser.Result.Parse("ba + bb");
            
            Assert.True(r.IsError);
            Assert.Single(r.Errors);
            var error = r.Errors.First() as UnexpectedTokenSyntaxError<Issue302Token>;
            Assert.NotNull(error);
            Assert.Equal(Issue302Token.PLUS,error.UnexpectedToken.TokenID);
        }
    }
}