using System;
using System.Linq;
using expressionparser;
using NFluent;
using ParserTests.Issue302;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.Issue302
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

            Check.That(r.IsOk).IsTrue();

            r = parser.Result.Parse("ba + bb");

            Check.That(r.IsError).IsTrue();
            Check.That(r.Errors).CountIs(1);
            var error = r.Errors.First() as UnexpectedTokenSyntaxError<Issue302Token>;
            Check.That(error).IsNotNull();
            Check.That(error.UnexpectedToken.TokenID).IsEqualTo(Issue302Token.PLUS);
            
        }
    }
}